using System.Collections.Generic;
using Newtonsoft.Json;

namespace Celebrities.FaceRecognitionService.ResultModels
{
    public class Result
    {
        [JsonProperty("faces")]
        public IList<Face> Faces { get; set; }
    }
}