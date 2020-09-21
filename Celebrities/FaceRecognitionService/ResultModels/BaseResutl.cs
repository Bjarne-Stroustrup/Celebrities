using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Celebrities.FaceRecognitionService.ResultModels
{
    public abstract class BaseResult
    {
        public bool IsSuccessful { get; set; }
    }
}
