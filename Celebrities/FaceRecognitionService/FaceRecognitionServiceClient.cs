using System.Net.Http;
using System.Threading.Tasks;
using Celebrities.FaceRecognitionService.ResultModels;
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
                $"{_configuration.GetSection($"{FaceRecognitionConfigSection}:EditFaceRelativeEndpoint").Value}";
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
            var result = new FacesRecognitionResult();
            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<FacesRecognitionResult>(responseString);
            result.IsSuccessful = true;

            return result;
        }

        public async Task<HttpResponseMessage> DeleteFaceExample(string faceName)
        {
            var relativeEndpoint =
                $"{_configuration.GetSection($"{FaceRecognitionConfigSection}:EditFaceRelativeEndpoint").Value}/?subject={faceName}";

            var response = await _httpClient.DeleteAsync(relativeEndpoint);
            return response;
        }
    }
}