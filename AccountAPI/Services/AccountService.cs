using AccountAPI.Entities;
using AccountAPI.Exceptions;
using AccountAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace AccountAPI.Services
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAccounts();
        Task<AccountDto> GetAccount(Guid id);
        Task<Guid> CreateAccount(CreateAccountDto dto);
        Task<bool> DeleteAccount(string Email);
        Task<bool> EditAccount(string Email, EditAccountDto dto);
        Task<AccountDto> GetAccountByEmail(string Email);
        Task<IEnumerable<AccountDto>> SearchUser(string userName);
    }
    public class AccountService : IAccountService
    {
        private readonly AccountDbContext _accountDbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Account> _passwordHasher;
        public AccountService(AccountDbContext accountDbContext, IMapper mapper, IPasswordHasher<Account> passwordHasher)
        {
            _accountDbContext = accountDbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<AccountDto>> GetAccounts()
        {
            var accounts = await _accountDbContext.Accounts
                .Include(a => a.Role)
                .ToListAsync();

            var accountsMap = _mapper.Map<List<AccountDto>>(accounts);

            return accountsMap;
        }

        public async Task<AccountDto> GetAccountByEmail(string Email)
        {
            var account = await _accountDbContext.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email == Email);

            var accountMap = _mapper.Map<AccountDto>(account);

            return accountMap;
        }

        public async Task<AccountDto> GetAccount(Guid id)
        {
            var account = await _accountDbContext.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == id);

            var accountMap = _mapper.Map<AccountDto>(account);

            return accountMap;
        }

        public async Task<IEnumerable<AccountDto>> SearchUser(string userName)
        {
            if (userName == null || userName.Length < 3)
            {
                throw new SearchPhraseException("Search phrase must be at least 3 characters long.");
            }

            var accounts = _accountDbContext.Accounts
                .Include(a => a.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userName))
            {
                var lowerCaseSearch = userName.ToLower();
                accounts = accounts.Where(a => a.Name.ToLower().Contains(lowerCaseSearch)
                || a.Email.Contains(lowerCaseSearch));
            }

            var listAccount = await accounts.ToListAsync();

            var accountMap = _mapper.Map<List<AccountDto>>(listAccount);

            return accountMap;
        }

        public async Task<Guid> CreateAccount(CreateAccountDto dto)
        {
            var newAccount = new Account() {
                Email = dto.Email,
                Name = dto.Name,
                RoleId = dto.RoleId,
            };
            var password = _passwordHasher.HashPassword(newAccount, dto.Password);
            newAccount.PasswordHash = password;


            _accountDbContext.Accounts.Add(newAccount);
            await _accountDbContext.SaveChangesAsync();

            return newAccount.Id;
        }

        public async Task<bool> DeleteAccount(string Email)
        {
            var account = await _accountDbContext.Accounts
                .FirstOrDefaultAsync(a => a.Email == Email);

            if (account == null)
            {
                return false;
            }

            _accountDbContext.Accounts.Remove(account);
            await _accountDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditAccount(string Email, EditAccountDto dto)
        {
            var account = await _accountDbContext.Accounts.FirstOrDefaultAsync(a => a.Email == Email);

            if (account == null)
            {
                return false;
            }

            _mapper.Map(dto, account);

            if (dto.RoleId.HasValue)
            {
                account.RoleId = dto.RoleId.Value;
            }

            account.WrongPasswordCounter = 0;

            await _accountDbContext.SaveChangesAsync();

            return true;
        }

        
    }
}
