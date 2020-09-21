using System.Net.Http;
using System.Threading.Tasks;
using Celebrities.FaceRecognitionService.ResultModels;
using Celebrities.FaceRecognitionService.ResultModels.FaceRecognitionResult;
using Celebrities.FaceRecognitionService.ResultModels.SavedImagesResult;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService
{
    public class FaceRecognitionServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FaceRecognitionServiceClient> _logger;
        private const string FaceRecognitionConfigSection = "Services:FaceRecognitionService";
        private const string FileContentName = "file";

        public FaceRecognitionServiceClient(HttpClient httpClient, IConfiguration configuration,
            ILogger<FaceRecognitionServiceClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> AddFaceExample(byte[] image, string fileName, string faceName)
        {
            var relativeEndpoint =
                $"{_configuration.GetSection($"{FaceRecognitionConfigSection}:FacesRelativeEndpoint").Value}";
            using var content = new MultipartFormDataContent
            {
                {new ByteArrayContent(image), FileContentName, fileName},
                {new StringContent(faceName), "\"subject\""}
            };

            var response = await _httpClient.PostAsync(relativeEndpoint, content);
            return response;
        }

        public async Task<FacesRecognitionResult> RecognizeFaces(byte[] image, string fileName)
        {
            var relativeEndpoint = _configuration
                .GetSection($"{FaceRecognitionConfigSection}:RecognizeFacesRelativeEndpoint").Value;
            using var content = new MultipartFormDataContent {{new ByteArrayContent(image), FileContentName, fileName}};

            var response = await _httpClient.PostAsync(relativeEndpoint, content);
            var result = await BuildResultFromResponse<FacesRecognitionResult>(response);
            return result;
        }

        public async Task<HttpResponseMessage> DeleteFaceExample(string faceName)
        {
            var relativeEndpoint =
                $"{_configuration.GetSection($"{FaceRecognitionConfigSection}:FacesRelativeEndpoint").Value}/?subject={faceName}";

            var response = await _httpClient.DeleteAsync(relativeEndpoint);
            return response;
        }

        public async Task<SavedImagesResult> GetListOfSavedImages()
        {
            var relativeEndpoint = _configuration.GetSection($"{FaceRecognitionConfigSection}:FacesRelativeEndpoint").Value;

            var response = await _httpClient.GetAsync(relativeEndpoint);
            var result = await BuildResultFromResponse<SavedImagesResult>(response);
            return result;
        }

        private async Task<T> BuildResultFromResponse<T> (HttpResponseMessage response) where T: BaseResult, new()
        {
            var result = new T();

            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T>(responseString);
            result.IsSuccessful = true;

            return result;
        }
    }
}