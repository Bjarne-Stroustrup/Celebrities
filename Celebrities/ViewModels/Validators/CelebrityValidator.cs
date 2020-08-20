using System.Linq;
using Celebrities.Database;
using FluentValidation;

namespace Celebrities.ViewModels.Validators
{
    public class CelebrityValidator : AbstractValidator<CelebrityViewModel>
    {
        public CelebrityValidator(CelebritiesDbContext celebritiesDbContext)
        {
            RuleFor(c => c.Name).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Please specify celebrity name")
                .Must(n => { return !celebritiesDbContext.Celebrities.Any(c => c.Name.Equals(n)); })
                .WithMessage(c => "The celebrity already exists");

            RuleFor(c => c.Avatar).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Please upload celebrity image")
                .SetValidator(new FormFileValidator());
        }
    }
}