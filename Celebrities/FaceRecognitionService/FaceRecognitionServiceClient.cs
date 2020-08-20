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
        private const string ContentName = "file";

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
                $"{_configuration.GetSection($"{FaceRecognitionConfigSection}:AddFaceRelativeEndpoint").Value}/{faceName}";
            using var content = new MultipartFormDataContent {{new ByteArrayContent(image), ContentName, fileName}};

            var response = await _httpClient.PostAsync(relativeEndpoint, content);
            return response;
        }

        public async Task<FacesRecognitionResult> RecognizeFaces(byte[] image, string fileName)
        {
            var relativeEndpoint = _configuration
                .GetSection($"{FaceRecognitionConfigSection}:RecognizeFacesRelativeEndpoint").Value;
            using var content = new MultipartFormDataContent {{new ByteArrayContent(image), ContentName, fileName}};

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
    }
}
