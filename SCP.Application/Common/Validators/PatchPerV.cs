using FluentValidation;
using SCP.Application.Core.Access;

namespace SCP.Application.Common.Validators
{
    public class PatchPerV : AbstractValidator<PatchPerCommand>
    {
        public PatchPerV()
        {
            _ = RuleFor(x => x.DayLife).NotEmpty().WithMessage("Укажите срок разрешения");
            _ = RuleFor(x => x.UserId).NotEmpty().WithMessage("Укажите пользователя");
            _ = RuleFor(x => x.SafeId).NotEmpty().WithMessage("Укажите сейф");

        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
