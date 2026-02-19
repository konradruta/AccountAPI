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
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
        {
            var accounts = await _accountService.GetAccounts();

            return Ok(accounts);
        }

        [HttpGet]
        [Route("byname")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> SearchUser(string name)
        {
            var accounts = await _accountService.SearchUser(name);

            return Ok(accounts);
        }

        [HttpGet("by-email/{email}")]
        public async Task<ActionResult> GetByEmail([FromRoute] string email)
        {
            var account = await _accountService.GetAccountByEmail(email);

            return Ok(account);
        }


        [HttpGet("by-id/{id:guid}")]
        public async Task<ActionResult> GetById([FromRoute] Guid id)
        {
            var account = await _accountService.GetAccount(id);

            return Ok(account);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Add([FromBody] CreateAccountDto dto)
        {
            await _accountService.CreateAccount(dto);

            return Ok();
        }

        [HttpDelete("{Email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete([FromRoute] string Email)
        {
            await _accountService.DeleteAccount(Email);

            return NoContent();
        }

        [HttpPut("{Email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([FromRoute] string Email, [FromBody] EditAccountDto dto)
        {
            await _accountService.EditAccount(Email, dto);

            return Ok();
        }
    }
}
