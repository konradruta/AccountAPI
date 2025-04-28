using AccountAPI.Entities;
using AccountAPI.Models;
using AccountAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccountAPI.Controllers
{
    [Authorize]
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        /*[Authorize(Roles = "1")]*/
        [AllowAnonymous]
        public ActionResult<IEnumerable<AccountDto>> GetAll()
        {
            var accounts = _accountService.GetAccounts();

            return Ok(accounts);
        }

        [HttpGet]
        [Route("byname")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<AccountDto>> SearchUser(string name)
        {
            var accounts = _accountService.SearchUser(name);

            return Ok(accounts);
        }

        [HttpGet("by-email/{email}")]
        public ActionResult GetByEmail([FromRoute] string email)
        {
            var account = _accountService.GetAccountByEmail(email);

            return Ok(account);
        }


        [HttpGet("by-id/{id:guid}")]
        public ActionResult GetById([FromRoute] Guid id)
        {
            var account = _accountService.GetAccount(id);

            return Ok(account);
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        public ActionResult Add([FromBody] CreateAccountDto dto)
        {
            _accountService.CreateAccount(dto);

            return Ok();
        }

        [HttpDelete("{Email}")]
        [Authorize(Roles = "1")]
        public ActionResult Delete([FromRoute] string Email)
        {
            _accountService.DeleteAccount(Email);

            return NoContent();
        }

        [HttpPut("{Email}")]
        [Authorize(Roles = "1")]
        public ActionResult Edit([FromRoute] string Email, [FromBody] EditAccountDto dto)
        {
            _accountService.EditAccount(Email, dto);

            return Ok();
        }
    }
}
