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
        IEnumerable<AccountDto> GetAccounts();
        AccountDto GetAccount(Guid id);
        Guid CreateAccount(CreateAccountDto dto);
        bool DeleteAccount(string Email);
        bool EditAccount(string Email, EditAccountDto dto);
        AccountDto GetAccountByEmail(string Email);
        IEnumerable<AccountDto> SearchUser(string userName);
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

        public IEnumerable<AccountDto> GetAccounts()
        {
            var accounts = _accountDbContext.Accounts
                .Include(a => a.Role)
                .ToList();

            var accountsMap = _mapper.Map<List<AccountDto>>(accounts);

            return accountsMap;
        }

        public AccountDto GetAccountByEmail(string Email)
        {
            var account = _accountDbContext.Accounts
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Email == Email);

            var accountMap = _mapper.Map<AccountDto>(account);

            return accountMap;
        }

        public AccountDto GetAccount(Guid id)
        {
            var account = _accountDbContext.Accounts
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Id == id);

            var accountMap = _mapper.Map<AccountDto>(account);

            return accountMap;
        }

        public IEnumerable<AccountDto> SearchUser(string userName)
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

            var listAccount = accounts.ToList();

            var accountMap = _mapper.Map<List<AccountDto>>(listAccount);

            return accountMap;
        }

        public Guid CreateAccount(CreateAccountDto dto)
        {
            var newAccount = new Account() {
                Email = dto.Email,
                Name = dto.Name,
                RoleId = dto.RoleId,
            };
            var password = _passwordHasher.HashPassword(newAccount, dto.Password);
            newAccount.PasswordHash = password;


            _accountDbContext.Accounts.Add(newAccount);
            _accountDbContext.SaveChanges();

            return newAccount.Id;
        }

        public bool DeleteAccount(string Email)
        {
            var account = _accountDbContext.Accounts
                .FirstOrDefault(a => a.Email == Email);

            if (account == null)
            {
                return false;
            }

            _accountDbContext.Accounts.Remove(account);
            _accountDbContext.SaveChanges();

            return true;
        }

        public bool EditAccount(string Email, EditAccountDto dto)
        {
            var account = _accountDbContext.Accounts.FirstOrDefault(a => a.Email == Email);

            if (account == null)
            {
                return false;
            }

            if (dto.Name != null)
            {
                account.Name = dto.Name;
            }

            if (dto.RoleId != 0)
            {
                account.RoleId = dto.RoleId;
            }

            account.WrongPasswordCounter = 0;

            _accountDbContext.SaveChanges();

            return true;
        }

        
    }
}
