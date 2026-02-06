using FluentValidation;

namespace AccountAPI.Models.Validators
{
    public class LoginChangePasswordValidator : AbstractValidator<LoginChangePassword>
    {
        public LoginChangePasswordValidator()
        {
            RuleFor(u => u.newPassword)
                .MinimumLength(6)
                .NotEmpty();

            RuleFor(u => u.confirmNewPassword)
                .Equal(p => p.newPassword).WithMessage("Passwords are diffrent");
        }
    }
}
