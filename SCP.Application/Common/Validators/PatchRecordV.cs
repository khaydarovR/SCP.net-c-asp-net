using FluentValidation;
using SCP.Application.Core.Record;

namespace SCP.Application.Common.Validators
{
    public class PatchRecordV : AbstractValidator<PatchRecordCommand>
    {
        public PatchRecordV()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Укажите название секрета");

        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
