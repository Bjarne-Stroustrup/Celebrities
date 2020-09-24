using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celebrities.Builders;
using Celebrities.Database;
using Celebrities.Database.Models;
using Celebrities.FaceRecognitionService;
using Celebrities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Celebrities.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CelebritiesDbContext _celebritiesDbContext;
        private readonly FaceRecognitionServiceClient _faceRecognitionServiceClient;
        private readonly CelebrityBuilder _celebrityBuilder;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            CelebritiesDbContext celebritiesDbContext,
            FaceRecognitionServiceClient faceRecognitionServiceClient,
            ILogger<AdminController> logger, CelebrityBuilder celebrityBuilder)
        {
            _celebritiesDbContext = celebritiesDbContext;
            _faceRecognitionServiceClient = faceRecognitionServiceClient;
            _celebrityBuilder = celebrityBuilder;
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
            celebrityDbModel.FaceRecognitionName = Guid.NewGuid().ToString();

            await _celebritiesDbContext.AddAsync(celebrityDbModel);
            await _celebritiesDbContext.SaveChangesAsync();

            return Ok(celebrityDbModel);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> AddCelebrityExample(int id, IFormFile image)
        {
            if (image == null)
            {
                return BadRequest();
            }

            var celebrity = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrity == null)
            {
                return NotFound();
            }

            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var faceRecognitionResponse = await _faceRecognitionServiceClient.AddFaceExample(memoryStream.ToArray(), image.FileName,
                    celebrity.FaceRecognitionName);

            if (!faceRecognitionResponse.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            if (!celebrity.Trained)
            {
                celebrity.Trained = true;
                await _celebritiesDbContext.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Celebrity>> DeleteCelebrity(int id)
        {
            var celebrity = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrity == null)
            {
                return NotFound();
            }

            if(celebrity.Trained) {
                var response = await _faceRecognitionServiceClient.DeleteFaceExample(celebrity.FaceRecognitionName);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest();
                }
            }

            _celebritiesDbContext.Celebrities.Remove(celebrity);
            await _celebritiesDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Celebrity>> UpdateCelebrity(int id, [FromForm]CelebrityViewModel celebrityViewModel)
        {
            if (celebrityViewModel == null)
            {
                return BadRequest();
            }

            var celebrityDb = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrityDb == null)
            {
                return NotFound();
            }

            await _celebrityBuilder.UpdateDbModelAsync(celebrityViewModel, celebrityDb);
            await _celebritiesDbContext.SaveChangesAsync();

            return Ok(celebrityDb);
        }


        [HttpGet("celebrityExampleCount/{id}")]
        public async Task<ActionResult<int>> GetCelebrityExampleCount(int id)
        {
            var celebrityDb = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == id);
            if (celebrityDb == null)
            {
                return NotFound();
            }

            var savedImagesResponse = await _faceRecognitionServiceClient.GetListOfSavedImages();
            if (!savedImagesResponse.IsSuccessful)
            {
                return BadRequest();
            }

            var count = savedImagesResponse.Faces.GroupBy(f => f.FaceName).Where(g => g.Key == celebrityDb.FaceRecognitionName)
                .Select(g => g.Count());

            return Ok(count);
        }

        [HttpGet("validation/doesCelebrityNameExist")]
        public async Task<bool> DoesCelebrityNameExist(string celebrityName)
        {
            return await _celebritiesDbContext.Celebrities.AnyAsync(c => c.Name == celebrityName);
        }
    }
}