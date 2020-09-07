using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celebrities.Builders;
using Celebrities.Database;
using Celebrities.FaceRecognitionService;
using Celebrities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Celebrities.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase
    {
        private readonly CelebritiesDbContext _celebritiesDbContext;
        private readonly FaceRecognitionServiceClient _faceRecognitionServiceClient;
        private readonly CelebrityBuilder _celebrityBuilder;
        private readonly ILogger<UserController> _logger;

        public UserController(
            CelebritiesDbContext celebritiesDbContext,
            FaceRecognitionServiceClient faceRecognitionServiceClient, CelebrityBuilder celebrityBuilder,
            ILogger<UserController> logger)
        {
            _celebritiesDbContext = celebritiesDbContext;
            _faceRecognitionServiceClient = faceRecognitionServiceClient;
            _celebrityBuilder = celebrityBuilder;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<SimilarCelebrityViewModel>>> RecognizeFace(IFormFile image)
        {
            if (image == null)
            {
                return BadRequest();
            }

            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var facesRecognitionResult = await _faceRecognitionServiceClient.RecognizeFaces(memoryStream.ToArray(), image.FileName);
            if (!facesRecognitionResult.IsSuccessful)
            {
                return BadRequest();
            }

            var similarFaces = facesRecognitionResult.Results.First().Faces.ToArray();
            var similarFaceNames = similarFaces.Select(f => f.FaceName).ToArray();
            var similarCelebrities = await _celebritiesDbContext.Celebrities.Where(c => similarFaceNames.Contains(c.Name))
                .ToArrayAsync();

            var viewModels = _celebrityBuilder.BuildSimilarCelebrityViewModels(similarFaces, similarCelebrities).ToArray();
            return viewModels;
        }
    }
}