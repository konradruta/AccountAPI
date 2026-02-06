using AccountAPI.Models;
using AccountAPI.Services;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult RegisterUser([FromBody] RegisterUserDto dto)
        {
            _userService.RegisterUser(dto);

            return Ok();
        }

        [HttpPost("login")]
        public ActionResult LoginUser([FromBody] LoginUserDto dto)
        {
            var token = _userService.LoginUser(dto);

            return Ok(token);
        }

        [HttpPost("changePassword")]
        public ActionResult ChangePassowrd([FromBody] ChangePasswordDto dto)
        {
            _userService.ChangePassword(dto);

            return Ok();
        }

        [HttpPost("forgotPassword")]
        public ActionResult ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            _userService.ForgotPassword(dto);

            return Ok($"New password is send on email {dto.Email}");
        }

        [HttpPost("LoginChangePassword")]
        public ActionResult LoginChangePassword([FromBody] LoginChangePassword dto)
        {
            _userService.ChangePassowrdLogin(dto);

            return Ok();
        }
    }
}
