using System;
using System.Net.Http.Headers;
using Celebrities.FaceRecognitionService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Celebrities.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFaceRecognitionServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<FaceRecognitionServiceClient>(cl =>
            {
                var faceRecognitionConfigSection = "Services:FaceRecognitionService";
                var baseFaceRecognitionEndpoint =
                    configuration.GetSection($"{faceRecognitionConfigSection}:BaseEndpoint").Value;
                var modelApiKey = configuration.GetSection($"{faceRecognitionConfigSection}:ModelApiKey").Value;

                cl.BaseAddress = new Uri(baseFaceRecognitionEndpoint);
                cl.DefaultRequestHeaders.Add("x-api-key", modelApiKey);
                cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }
}