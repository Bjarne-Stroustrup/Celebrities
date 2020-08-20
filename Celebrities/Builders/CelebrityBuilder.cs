using System.IO;
using System.Threading.Tasks;
using Celebrities.Database.Models;
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
    }
}
