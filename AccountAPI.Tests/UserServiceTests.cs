using AccountAPI.Entities;
using AccountAPI.Exceptions;
using AccountAPI.Models;
using AccountAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AccountAPI.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IPasswordHasher<Account>> _passwordHasherMock = new();
        private readonly Mock<IEmailSender> _emailSenderMock = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

        private UserService CreateService(AccountDbContext context)
        {
            var settings = new AuthenticationSettings
            {
                JwtKey = "test_key_test_key_test_key_test_key",
                JwtIssuer = "test",
                JwtExpiresDays = 1
            };

            return new UserService(
                context,
                _passwordHasherMock.Object,
                settings,
                _emailSenderMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public void RegisterUser_ShouldAddUserToDatabase()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: "RegisterUserDb")
                .Options;

            var context = new AccountDbContext(options);

            _passwordHasherMock
                .Setup(h => h.HashPassword(It.IsAny<Account>(), It.IsAny<string>()))
                .Returns("hashed");

            var service = CreateService(context);

            var dto = new RegisterUserDto()
            {
                Email = "test@test.com",
                Name = "Test",
                Password = "Password123"
            };

            service.RegisterUser(dto);

            Assert.Single(context.Accounts);
        }

        [Fact]
        public void LoginUser_ShouldThrowException_WhenUserNotFound()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: "LoginUserDb1")
                .Options;

            var context = new AccountDbContext(options);

            var service = CreateService(context);

            var dto = new LoginUserDto
            {
                Email = "notfound@test.com",
                Password = "Password123"
            };

            Assert.Throws<WrongLoginException>(() => service.LoginUser(dto));
        }

        [Fact]
        public void LoginUser_ShouldReturnToken_WhenCredentialsCorrect()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: "LoginUserDb2")
                .Options;

            var context = new AccountDbContext(options);

            var role = new Role { Id = 1, Name = "User" };

            var user = new Account
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Name = "Test",
                PasswordHash = "hashed",
                Role = role,
                WrongPasswordCounter = 0
            };

            context.Accounts.Add(user);
            context.SaveChanges();

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(
                    It.IsAny<Account>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            var service = CreateService(context);

            var dto = new LoginUserDto
            {
                Email = "test@test.com",
                Password = "Password123"
            };

            var token = service.LoginUser(dto);

            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void LoginUser_ShouldThrowException_WhenPasswordIncorrect()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: "LoginUserDb3")
                .Options;

            var context = new AccountDbContext(options);

            var role = new Role { Id = 1, Name = "User" };

            var user = new Account
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Name = "Test",
                PasswordHash = "hashed",
                Role = role,
                WrongPasswordCounter = 0
            };

            context.Accounts.Add(user);
            context.SaveChanges();

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(
                    It.IsAny<Account>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            var service = CreateService(context);

            var dto = new LoginUserDto
            {
                Email = "test@test.com",
                Password = "WrongPassword"
            };

            Assert.Throws<WrongLoginException>(() => service.LoginUser(dto));
        }
    }
}