using Microsoft.AspNetCore.Http;

namespace Celebrities.ViewModels
{
    public class CelebrityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public IFormFile Avatar { get; set; }
        public bool Trained { get; set; }
    }
}