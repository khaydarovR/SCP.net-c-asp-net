using FluentValidation;
using SCP.Application.Core.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Common.Validators
{
    public class CreateRecordV : AbstractValidator<CreateRecordCommand>
    {
        public CreateRecordV()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Укажите название секрета");

            RuleFor(x => x.SafeId)
               .NotEmpty().WithMessage("Не указан сейф");
        }

        private bool BeAValidPostcode(string postcode)
        {
            return true;
        }
    }
}
