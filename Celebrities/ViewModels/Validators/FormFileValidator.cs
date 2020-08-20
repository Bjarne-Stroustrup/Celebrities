using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Celebrities.ViewModels.Validators
{
    public class FormFileValidator : AbstractValidator<IFormFile>
    {
        private readonly string[] _permittedExtensions = { ".jpeg", ".jpg", ".ico", ".png", ".bmp", ".gif", ".tif", ".tiff", ".webp" };
        public FormFileValidator()
        {
            CascadeMode = CascadeMode.Stop;
            RuleFor(f => f.FileName).Must(f =>
            {
                var fileExtension = Path.GetExtension(f).ToLowerInvariant();
                return !string.IsNullOrEmpty(fileExtension) && _permittedExtensions.Contains(fileExtension);
            }).WithMessage($"Please upload image of the valid extension: {string.Join(" ,", _permittedExtensions)}");

            RuleFor(f => f.Length).LessThanOrEqualTo(5242880)
                .WithMessage("The size of the image must be 5 MB or less");
        }
    }
}