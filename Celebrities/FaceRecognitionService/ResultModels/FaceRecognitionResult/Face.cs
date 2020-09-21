using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels.FaceRecognitionResult
{
    public class Face
    {
        [JsonProperty("subject")]
        public string FaceName { get; set; }

        [JsonProperty("similarity")]
        public decimal Similarity { get; set; }
    }
}