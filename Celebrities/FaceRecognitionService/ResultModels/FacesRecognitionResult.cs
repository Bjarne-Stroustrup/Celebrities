using System.Collections.Generic;
using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels
{
    public class FacesRecognitionResult
    {
        public bool IsSuccessful { get; set; }

        [JsonProperty("result")]
        public IList<Result> Results { get; set; }
    }
}