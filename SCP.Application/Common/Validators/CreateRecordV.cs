using FluentValidation;
using SCP.Application.Core.Record;

namespace SCP.Application.Common.Validators
{
    public class CreateRecordV : AbstractValidator<CreateRecordCommand>
    {
        public CreateRecordV()
        {
            _ = RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Укажите название секрета");

            _ = RuleFor(x => x.SafeId)
               .NotEmpty().WithMessage("Не указан сейф");
        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
