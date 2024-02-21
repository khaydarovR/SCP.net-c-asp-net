using FluentValidation;
using SCP.Application.Core.Access;

namespace SCP.Application.Common.Validators
{
    public class InviteUsersToSafesV : AbstractValidator<AuthrizeUsersToSafeCommand>
    {
        public InviteUsersToSafesV()
        {
            _ = RuleFor(x => x.SafeIds).NotEmpty().WithMessage("Укажите для каких сейфов");
            _ = RuleFor(x => x.UserIds).NotEmpty().WithMessage("Укажите для каких пользователей");
            _ = RuleFor(x => x.DayLife).NotEmpty().WithMessage("Укажите срок годности выдаваемых разрешений");
        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
