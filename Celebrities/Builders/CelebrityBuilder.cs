using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celebrities.Database.Models;
using Celebrities.FaceRecognitionService.ResultModels;
using Celebrities.ViewModels;

namespace Celebrities.Builders
{
    public class CelebrityBuilder
    {
        public async Task<Celebrity> BuildDbModelAsync(CelebrityViewModel celebrityViewModel)
        {
            await using var memoryStream = new MemoryStream();
            await celebrityViewModel.Avatar.CopyToAsync(memoryStream);

            return new Celebrity
            {
                Id = celebrityViewModel.Id,
                Name = celebrityViewModel.Name,
                Info = celebrityViewModel.Info,
                ImageName = celebrityViewModel.Avatar.FileName,
                AvatarImage = memoryStream.ToArray()
            };
        }

        public IEnumerable<SimilarCelebrityViewModel> BuildSimilarCelebrityViewModels(IEnumerable<Face> faces, IEnumerable<Celebrity> celebritiesDb)
        {
            var viewModels = faces.Join(celebritiesDb,
                f => f.FaceName,
                c => c.Name, (f, c) => new {c.Name, c.Info, f.Similarity, c.AvatarImage}).Select(a =>
                new SimilarCelebrityViewModel
                {
                    Name = a.Name,
                    Info = a.Info,
                    Similarity = a.Similarity,
                    Image = a.AvatarImage
                });
            return viewModels;
        }
    }
}