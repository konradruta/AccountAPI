using AccountAPI.Entities;
using AccountAPI.Exceptions;
using AccountAPI.Migrations;
using AccountAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccountAPI.Services
{
    public interface IUserService
    {
        void RegisterUser(RegisterUserDto dto);
        string LoginUser(LoginUserDto dto);
        void ChangePassword(ChangePasswordDto dto);
        void ForgotPassword(ForgotPasswordDto dto);
        void ChangePassowrdLogin(LoginChangePassword dto);
    }
    public class UserService : IUserService
    {
        private readonly AccountDbContext _accountDb;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IEmailSender _emailServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(AccountDbContext accountDb, IPasswordHasher<Account> passwordHasher, AuthenticationSettings authenticationSettings, IEmailSender emailServices, IHttpContextAccessor httpContextAccessor)
        {
            _accountDb = accountDb;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
            _httpContextAccessor = httpContextAccessor;
            _emailServices = emailServices;
        }

        public void RegisterUser(RegisterUserDto dto)
        {
            var newUser = new Account()
            {
                Email = dto.Email,
                Name = dto.Name,
            };
            var hashedPassword = _passwordHasher.HashPassword(newUser, dto.Password);
            newUser.PasswordHash = hashedPassword;

            _accountDb.Add(newUser);
            _accountDb.SaveChanges();
        }

        public string LoginUser(LoginUserDto dto)
        {
            var user = _accountDb.Accounts
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new WrongLoginException("Wrong email or password");
            }
            
            //Sprawdzenie blokady użytkownika oraz odblokowanie po 15 minutach od ostatniej błędnej próby logowania
            if (user.LastFailedLoginAttempt.HasValue &&
                user.LastFailedLoginAttempt.Value.AddMinutes(15) < DateTime.UtcNow)
            {
                user.WrongPasswordCounter = 0;
                _accountDb.SaveChanges();
            }

            if (user.WrongPasswordCounter >= 4)
            {
                throw new WrongLoginException("Your account is temporary blocked. To unlock your account change password or wait 15 minutes.");
            }

            var passwordVerify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (passwordVerify == PasswordVerificationResult.Failed)
            {
                user.WrongPasswordCounter++;
                user.LastFailedLoginAttempt = DateTime.UtcNow;
                _accountDb.SaveChanges();
                throw new WrongLoginException("Wrong email or password");
            }

            //Resetowanie licznika błędnych logowań
            user.WrongPasswordCounter = 0;
            user.LastFailedLoginAttempt = null;
            _accountDb.SaveChanges();

            if (user.IsPasswordTemporary == true)
            {
                throw new TemporaryPasswordException("You have to chagne password");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpiresDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims: claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(token);
        }

        public void ChangePassword(ChangePasswordDto dto)
        {
            var user = _accountDb.Accounts.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new NotUserFoundException("User with that e-mail dosen't exist.");
            }

            var passwordVerify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.oldPassword);

            if (passwordVerify == PasswordVerificationResult.Failed)
            {
                throw new WrongLoginException("Old password are wrong.");
            }

            var newPasswordHash = _passwordHasher.HashPassword(user, dto.newPassword);

            if (dto.newPassword == dto.oldPassword)
            {
                throw new WrongLoginException("The password can't be the same.");
            }

            user.PasswordHash = newPasswordHash;

            //Resetowanie licznika błędnych logowań oraz usunięcie wymogu zmiany hasła przy logowaniu
            user.WrongPasswordCounter = 0;
            user.IsPasswordTemporary = false;


            _accountDb.SaveChanges();
        }

        public void ChangePassowrdLogin(LoginChangePassword dto)
        {
            var userEmail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = _accountDb.Accounts.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                throw new NotUserFoundException("User not found.");
            }

            var passwordVerify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.oldPassword);

            if (passwordVerify == PasswordVerificationResult.Failed)
            {
                throw new WrongLoginException("Old password is incorrect.");
            }

            if (dto.newPassword == dto.oldPassword)
            {
                throw new WrongLoginException("New password cannot be the same as the old password.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.newPassword);
            user.WrongPasswordCounter = 0;
            user.IsPasswordTemporary = false;

            _accountDb.SaveChanges();
        }

        public void ForgotPassword(ForgotPasswordDto dto)
        {
            var user = _accountDb.Accounts.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new NotUserFoundException("User with that e-mail dosen't exist.");
            }

            if (user.Name != dto.Name)
            {
                throw new NotUserFoundException("User with that Name dosen't exist.");
            }

            /*if (dto.AccteptSend == false)
            {
                throw new NotUserFoundException("You need accept send e-mail.");
            }*/

            var newPassword = GenerateRandomPassword(8);

            var newTempPasswordHash = _passwordHasher.HashPassword(user, newPassword);

            //Ustawienie nowego hasła jako tymczasowego oraz reset licznika błędnych logowań
            user.PasswordHash = newTempPasswordHash;
            user.IsPasswordTemporary = true;
            user.WrongPasswordCounter = 0;
            _accountDb.SaveChanges();

            var subject = "Your new password";
            var body = $"Hello {user.Name}, \n\nYour new temporary password is: {newPassword}";

            _emailServices.SendEmail(user.Email, subject, body);

            /*return newPassword;*/
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string specialChars = "!@#$%^&*()";

            if (length < 3) // Zapewniamy miejsce na cyfrę, znak specjalny i inne znaki
                throw new ArgumentException("Password length must be at least 3.");

            var random = new Random();

            // Losowanie przynajmniej jednej cyfry i znaku specjalnego
            var password = new StringBuilder();
            password.Append(validChars[random.Next(10, validChars.Length)]); // Cyfra
            password.Append(specialChars[random.Next(specialChars.Length)]); // Znak specjalny

            // Losowanie pozostałych znaków
            for (int i = 2; i < length; i++)
            {
                var charsToChoose = random.Next(2) == 0 ? validChars : specialChars;
                password.Append(charsToChoose[random.Next(charsToChoose.Length)]);
            }

            // Mieszamy hasło, aby losowość była pełna
            return new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
        }
    }
}
