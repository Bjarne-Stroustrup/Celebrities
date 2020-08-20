using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels
{
    public class Face
    {
        [JsonProperty("face_name")]
        public string FaceName { get; set; }

        [JsonProperty("similarity")]
        public decimal Similarity { get; set; }
    }
}