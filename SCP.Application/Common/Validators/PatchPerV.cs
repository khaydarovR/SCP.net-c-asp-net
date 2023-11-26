using FluentValidation;
using SCP.Application.Core.Access;

namespace SCP.Application.Common.Validators
{
    public class PatchPerV : AbstractValidator<PatchPerCommand>
    {
        public PatchPerV()
        {
            RuleFor(x => x.DayLife).NotEmpty().WithMessage("Укажите срок разрешения");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Укажите пользователя");
            RuleFor(x => x.SafeId).NotEmpty().WithMessage("Укажите сейф");

        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
