using FluentValidation;

namespace AccountAPI.Models.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(u => u.Email)
                .EmailAddress()
                .NotEmpty();

            RuleFor(u => u.newPassword)
                .MinimumLength(6)
                .NotEmpty();

            RuleFor(u => u.confirmNewPassword)
                .Equal(p => p.newPassword).WithMessage("The new passwords are diffrents");
        }
    }
}
