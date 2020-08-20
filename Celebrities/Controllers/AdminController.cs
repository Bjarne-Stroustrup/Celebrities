using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Celebrities.Builders;
using Celebrities.Database;
using Celebrities.Database.Models;
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
    public class AdminController : ControllerBase
    {
        private readonly CelebritiesDbContext _celebritiesDbContext;
        private readonly FaceRecognitionServiceClient _faceRecognitionServiceClient;
        private readonly CelebrityBuilder _celebrityBuilder = new CelebrityBuilder();
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            CelebritiesDbContext celebritiesDbContext,
            FaceRecognitionServiceClient faceRecognitionServiceClient, 
            ILogger<AdminController> logger)
        {
            _celebritiesDbContext = celebritiesDbContext;
            _faceRecognitionServiceClient = faceRecognitionServiceClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Celebrity>> GetAllCelebrities()
        {
            return await _celebritiesDbContext.Celebrities.ToArrayAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Celebrity>> GetCelebrity(int id)
        {
            var celebrity = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrity == null)
            {
                return NotFound();
            }

            return new ObjectResult(celebrity);
        }

        [HttpPost]
        public async Task<ActionResult<Celebrity>> AddCelebrity([FromForm] CelebrityViewModel celebrityViewModel)
        {
            var celebrityDbModel = await _celebrityBuilder.BuildDbModelAsync(celebrityViewModel);
            await _celebritiesDbContext.AddAsync(celebrityDbModel);
            await _celebritiesDbContext.SaveChangesAsync();

            return Ok(celebrityDbModel);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> AddCelebrityExample(int id, IFormFile image)
        {
            var celebrity = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrity == null)
            {
                return NotFound();
            }

            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var faceRecognitionResponse = await _faceRecognitionServiceClient.AddFaceExample(memoryStream.ToArray(), image.FileName,
                    celebrity.Name);

            //TODO [Julia] Think about uploading several faces one more image
            if (!faceRecognitionResponse.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            if (!celebrity.Trained)
            {
                celebrity.Trained = true;
                _celebritiesDbContext.Celebrities.Update(celebrity);
                await _celebritiesDbContext.SaveChangesAsync();
            }

            return Ok();
        }
    }
}