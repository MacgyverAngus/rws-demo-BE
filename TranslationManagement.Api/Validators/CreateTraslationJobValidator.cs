using FluentValidation;
using TranslationManagement.Api.DataContracts;

namespace TranslationManagement.Api.Validators
{
    public class CreateTraslationJobValidator : AbstractValidator<CreateTranslationJobDto>
    {
        public CreateTraslationJobValidator()
        {
            RuleFor(x => x.CustomerName).NotNull().NotEmpty();
            RuleFor(x => x.TranslatedContent).NotNull().NotEmpty();
            RuleFor(x => x.OriginalContent).NotNull().NotEmpty();
        }
    }
}
