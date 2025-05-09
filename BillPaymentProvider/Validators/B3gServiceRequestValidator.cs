using BillPaymentProvider.Core.Models;
using FluentValidation;

namespace BillPaymentProvider.Validators
{
    public class B3gServiceRequestValidator : AbstractValidator<B3gServiceRequest>
    {
        public B3gServiceRequestValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
            RuleFor(x => x.ServiceId).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Language).NotEmpty();
            RuleFor(x => x.ChannelId).NotEmpty();
            RuleFor(x => x.ParamIn).NotNull();
        }
    }
}
