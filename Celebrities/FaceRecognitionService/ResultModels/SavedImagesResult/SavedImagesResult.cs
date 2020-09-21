using System.Collections.Generic;
using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels.SavedImagesResult
{
    public class SavedImagesResult : BaseResult
    {
        [JsonProperty("faces")]
        public IList<Face> Faces { get; set; }
    }
}