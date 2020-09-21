using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels.SavedImagesResult
{
    public class Face
    {
        [JsonProperty("image_id")] 
        public string ImageId { get; set; }

        [JsonProperty("subject")]
        public string FaceName { get; set; }
    }
}