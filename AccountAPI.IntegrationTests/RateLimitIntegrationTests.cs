using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AccountAPI.IntegrationTests
{
    public class RateLimitIntegrationTests : IClassFixture<AccountApiFactoryWithRateLimit>
    {
        private readonly HttpClient _client;

        public RateLimitIntegrationTests(
            AccountApiFactoryWithRateLimit factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Should_Return_429_When_Limit_Exceeded()
        {
            var email = $"{Guid.NewGuid()}@test.com";

            var register = new
            {
                email,
                name = "Test",
                password = "Password123!",
                confirmPassword = "Password123!"
            };

            await _client.PostAsJsonAsync("/api/user/register", register);

            var login = new
            {
                email,
                password = "Password123!"
            };

            HttpResponseMessage? response = null;

            for (int i = 0; i < 11; i++)
            {
                response = await _client.PostAsJsonAsync(
                    "/api/user/login",
                    login);
            }

            response!.StatusCode
                .Should()
                .Be(HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task Refresh_Should_Return_429_When_Limit_Exceeded()
        {
            var email = $"{Guid.NewGuid()}@test.com";

            var register = new
            {
                email,
                name = "Test123",
                password = "Password123!",
                confirmPassword = "Password123!"
            };

            await _client.PostAsJsonAsync("/api/user/register", register);

            var login = new
            {
                email,
                password = "Password123!"
            };

            HttpResponseMessage? response = null;

            for (int i = 0; i < 16; i++)
            {
                response = await _client.PostAsJsonAsync(
                    "/api/user/refresh",
                    login);
            }

            response!.StatusCode
                .Should()
                .Be(HttpStatusCode.TooManyRequests);
        }
    }
}
