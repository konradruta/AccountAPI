using AccountAPI.Models;
using AccountAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace AccountAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserDto dto)
        {
            await _userService.RegisterUser(dto);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginUserDto dto)
        {
            var token = await _userService.LoginUser(dto);

            return Ok(token);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var tokens = await _userService.RefreshToken(dto);

            return Ok(tokens);
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new { email });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _userService.Logout();

            return Ok();
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult> ChangePassowrd([FromBody] ChangePasswordDto dto)
        {
            await _userService.ChangePassword(dto);

            return Ok();
        }

        [HttpPost("forgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _userService.ForgotPassword(dto);

            return Ok($"New password is send on email {dto.Email}");
        }

        [Authorize]
        [HttpPost("LoginChangePassword")]
        public async Task<ActionResult> LoginChangePassword([FromBody] LoginChangePassword dto)
        {
            await _userService.ChangePassowrdLogin(dto);

            return Ok();
        }
    }
}
