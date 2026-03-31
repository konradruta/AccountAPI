using AccountAPI.Entities;
using AccountAPI.Exceptions;
using AccountAPI.Migrations;
using AccountAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AccountAPI.Services
{
    public interface IUserService
    {
        Task RegisterUser(RegisterUserDto dto);
        Task<AuthResponseDto> LoginUser(LoginUserDto dto);
        Task<AuthResponseDto> RefreshToken(RefreshTokenDto dto);
        Task Logout();
        Task ChangePassword(ChangePasswordDto dto);
        Task ForgotPassword(ForgotPasswordDto dto);
        Task ChangePassowrdLogin(LoginChangePassword dto);
    }
    public class UserService : IUserService
    {
        private readonly AccountDbContext _accountDb;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IEmailSender _emailServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        public UserService(AccountDbContext accountDb, IPasswordHasher<Account> passwordHasher, AuthenticationSettings authenticationSettings, IEmailSender emailServices, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _accountDb = accountDb;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
            _httpContextAccessor = httpContextAccessor;
            _emailServices = emailServices;
            _logger = logger;
        }

        public async Task RegisterUser(RegisterUserDto dto)
        {
            var newUser = new Account()
            {
                Email = dto.Email,
                Name = dto.Name,
            };
            var hashedPassword = _passwordHasher.HashPassword(newUser, dto.Password);
            newUser.PasswordHash = hashedPassword;

            _accountDb.Add(newUser);

            _logger.LogWarning($"User {dto.Email} created account");

            await _accountDb.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> LoginUser(LoginUserDto dto)
        {
            var user = await _accountDb.Accounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new WrongLoginException("Wrong email or password");
            }
            
            //Sprawdzenie blokady użytkownika oraz odblokowanie po 15 minutach od ostatniej błędnej próby logowania
            if (user.LastFailedLoginAttempt.HasValue &&
                user.LastFailedLoginAttempt.Value.AddMinutes(15) < DateTime.UtcNow)
            {
                user.WrongPasswordCounter = 0;
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
                await _accountDb.SaveChangesAsync();
                throw new WrongLoginException("Wrong email or password");
            }

            //Resetowanie licznika błędnych logowań
            user.WrongPasswordCounter = 0;
            user.LastFailedLoginAttempt = null;

            if (user.IsPasswordTemporary == true)
            {
                throw new TemporaryPasswordException("You have to change password");
            }

            var accessToken = GenerateJwtToken(user);

            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime  = DateTime.UtcNow.AddDays(7);

            await _accountDb.SaveChangesAsync();

            _logger.LogWarning($"User {dto.Email} is logged");

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateJwtToken(Account user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_authenticationSettings.JwtExpiresDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims: claims,
                expires: expires,
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<AuthResponseDto> RefreshToken(RefreshTokenDto dto)
        {
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = await _accountDb.Accounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _accountDb.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            return principal;
        }

        public async Task Logout()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException();
            }

            var user = await _accountDb.Accounts.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _accountDb.SaveChangesAsync();
        }

        public async Task ChangePassword(ChangePasswordDto dto)
        {
            var user = await _accountDb.Accounts.FirstOrDefaultAsync(u => u.Email == dto.Email);

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

            _logger.LogWarning($"User {dto.Email} changed password");

            await _accountDb.SaveChangesAsync();
        }

        public async Task ChangePassowrdLogin(LoginChangePassword dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await _accountDb.Accounts.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

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

            _logger.LogWarning($"User {user.Email} changed password");

            await _accountDb.SaveChangesAsync();
        }

        public async Task ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _accountDb.Accounts.FirstOrDefaultAsync(u => u.Email == dto.Email);

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
            await _accountDb.SaveChangesAsync();

            var subject = "Your new password";
            var body = $"Hello {user.Name}, \n\nYour new temporary password is: {newPassword}";

            _logger.LogWarning($"User {user.Email} request new password");

            await _emailServices.SendEmail(user.Email, subject, body);

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
