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
            celebrityDbModel.FaceRecognitionName = celebrityViewModel.Name;
            celebrityDbModel.Trained = false;

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
                _celebritiesDbContext.Celebrities.Update(celebrity);
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

        //Don't check this, it doesn't work :) Need to figure out what's wrong and fix
        [HttpPut("id")]
        public async Task<ActionResult<Celebrity>> EditCelebrity([FromForm]CelebrityViewModel celebrityViewModel, int id)
        {
            if (celebrityViewModel == null)
            {
                return BadRequest();
            }

            var celebrity = await _celebritiesDbContext.Celebrities.FirstOrDefaultAsync(c => c.Id == celebrityViewModel.Id);
            if (celebrity == null)
            {
                return NotFound();
            }

            var celebrityDb = await _celebrityBuilder.BuildDbModelAsync(celebrityViewModel);
            celebrityDb.FaceRecognitionName = celebrity.FaceRecognitionName;
            celebrityDb.Trained = celebrity.Trained;

            _celebritiesDbContext.Celebrities.Update(celebrityDb);
            await _celebritiesDbContext.SaveChangesAsync();

            return Ok(celebrityViewModel);
        }

        [HttpGet("validation/doesCelebrityNameExist")]
        public async Task<bool> DoesCelebrityNameExist(string celebrityName)
        {
            return await _celebritiesDbContext.Celebrities.AnyAsync(c => c.Name == celebrityName);
        }
    }
}