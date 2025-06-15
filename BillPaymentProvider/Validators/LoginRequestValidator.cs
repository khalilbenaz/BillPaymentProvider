using FluentValidation;

namespace BillPaymentProvider.Validators
{
    public class LoginRequestValidator : AbstractValidator<BillPaymentProvider.Controllers.LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
