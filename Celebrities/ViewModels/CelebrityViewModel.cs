using Microsoft.AspNetCore.Http;

namespace Celebrities.ViewModels
{
    public class CelebrityViewModel
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public IFormFile Avatar { get; set; }
    }
}