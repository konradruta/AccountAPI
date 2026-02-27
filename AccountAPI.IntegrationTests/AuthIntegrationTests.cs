using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace AccountAPI.IntegrationTests
{
    public class AuthIntegrationTests : IClassFixture<AccountApiFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(AccountApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_Return_OK()
        {
            var email = $"{Guid.NewGuid()}@test.com";

            var request = new
            {
                email,
                name = "Testowy",
                password = "Password123!",
                confirmPassword = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/user/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_Should_Return_Jwt()
        {
            var email = $"{Guid.NewGuid()}@test.com";

            // REGISTER
            var registerRequest = new
            {
                email,
                name = "Testowy2",
                password = "Password123!",
                confirmPassword = "Password123!"
            };

            await _client.PostAsJsonAsync("/api/user/register", registerRequest);

            // LOGIN
            var request = new
            {
                email,
                password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/user/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result!.RefreshToken.Should().NotBeNullOrEmpty();
        }


        public class LoginResponse
        {
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
        }

        [Fact]
        public async Task GetMe_With_Token_Should_Return_200()
        {
            var email = $"{Guid.NewGuid()}@test.com";

            // REGISTER
            var registerRequest = new
            {
                email,
                name = "Test User",
                password = "Password123!",
                confirmPassword = "Password123!",
            };

            await _client.PostAsJsonAsync("/api/user/register", registerRequest);

            // LOGIN
            var loginRequest = new
            {
                email,
                password = "Password123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/user/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // ADD JWT
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

            // CALL ME
            var response = await _client.GetAsync("/api/user/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Protected_Endpoint_Without_Token_Should_Return_401()
        {
            var response = await _client.GetAsync("/api/user/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
