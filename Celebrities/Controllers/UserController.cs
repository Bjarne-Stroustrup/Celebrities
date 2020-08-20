using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celebrities.Builders;
using Celebrities.Database;
using Celebrities.Database.Models;
using Celebrities.FaceRecognitionService;
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
        private readonly ILogger<UserController> _logger;

        public UserController(
            CelebritiesDbContext celebritiesDbContext,
            FaceRecognitionServiceClient faceRecognitionServiceClient,
            ILogger<UserController> logger)
        {
            _celebritiesDbContext = celebritiesDbContext;
            _faceRecognitionServiceClient = faceRecognitionServiceClient;
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<Celebrity>>> RecognizeFace(IFormFile image)
        {
            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var facesRecognitionResult = await _faceRecognitionServiceClient.RecognizeFaces(memoryStream.ToArray(), image.FileName);
            if (!facesRecognitionResult.IsSuccessful)
            {
                return BadRequest();
            }

            //TODO [Julia] Think about recognizing several faces on the image
            var similarFaceNames = facesRecognitionResult.Results.First().Faces.Select(f => f.FaceName).ToArray();
            var similarCelebrities = await _celebritiesDbContext.Celebrities.Where(c => similarFaceNames.Contains(c.Name))
                .ToArrayAsync();
            return similarCelebrities;
        }
    }
}
