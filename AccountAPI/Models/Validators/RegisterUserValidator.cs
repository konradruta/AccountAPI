using AccountAPI.Entities;
using FluentValidation;

namespace AccountAPI.Models.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserValidator(AccountDbContext dbContext)
        {
            RuleFor(u => u.Password)
                .MinimumLength(8)
                .NotEmpty();

            RuleFor(u => u.ConfirmPassword)
                .MinimumLength(8)
                .Equal(p => p.Password).WithMessage("The password are diffrents");

            RuleFor(u => u.Name)
                .Custom((value, context) =>
                {
                    var takenName = dbContext.Accounts.Any(u => u.Name == value);

                    if (takenName)
                    {
                        context.AddFailure("That username is taken");
                    }
                });

            RuleFor(u => u.Email)
                .Custom((value, context) =>
                {
                    var takenEmail = dbContext.Accounts.Any(u => u.Email == value);

                    if (takenEmail)
                    {
                        context.AddFailure("That e-mail is taken");
                    }
                });
        }
    }
}
