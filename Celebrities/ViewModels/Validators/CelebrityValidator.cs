using System.Linq;
using Celebrities.Database;
using FluentValidation;

namespace Celebrities.ViewModels.Validators
{
    public class CelebrityValidator : AbstractValidator<CelebrityViewModel>
    {
        public CelebrityValidator(CelebritiesDbContext celebritiesDbContext)
        {
            RuleFor(celebrityViewModel => celebrityViewModel).Cascade(CascadeMode.Stop)
                .Must(celebrityViewModel => celebrityViewModel.Name != null)
                    .WithMessage("Please specify celebrity name").Must(celebrityViewModel =>
                    {
                        if (celebrityViewModel.Id.HasValue)
                        {
                            return !celebritiesDbContext.Celebrities.Any(c =>
                                c.Name.Equals(celebrityViewModel.Name) && c.Id != celebrityViewModel.Id.Value);
                        }

                        return !celebritiesDbContext.Celebrities.Any(c => c.Name.Equals(celebrityViewModel.Name));
                    })
                    .WithMessage(c => "The celebrity already exists");

            //RuleFor(c => c.Avatar).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Please upload celebrity image")
            //    .SetValidator(new FormFileValidator());
        }
    }
}