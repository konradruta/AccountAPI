using AccountAPI.Entities;
using FluentValidation;

namespace AccountAPI.Models.Validators
{
    public class AddUserValidator : AbstractValidator<CreateAccountDto>
    {
        public AddUserValidator(AccountDbContext dbContext)
        {
            RuleFor(a => a.Email)
                .EmailAddress()
                .NotEmpty();

            RuleFor(a => a.Email)
                .Custom((value, context) =>
                {
                    var TakenEmail = dbContext.Accounts.Any(e => e.Email == value);
                    if (TakenEmail)
                    {
                        context.AddFailure("That e-mail is taken");
                    }
                }
            );

            RuleFor(a => a.Password)
                .MinimumLength(8)
                .NotEmpty();

            RuleFor(u => u.Name)
                .Custom((value, context) =>
                {
                    var takenName = dbContext.Accounts.Any(u => u.Name == value);

                    if (takenName)
                    {
                        context.AddFailure("That username is taken");
                    }
                });

        }
    }
}
