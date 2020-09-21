using System.Collections.Generic;
using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels.FaceRecognitionResult
{
    public class FacesRecognitionResult: BaseResult
    {
        [JsonProperty("result")]
        public IList<Result> Results { get; set; }
    }
}