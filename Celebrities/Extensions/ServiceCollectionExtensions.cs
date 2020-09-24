using System;
using System.Net.Http.Headers;
using System.Text;
using Celebrities.FaceRecognitionService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = configuration.GetSection("Authentication:Secret").Value;
                    var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = symmetricSecurityKey,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };
                });

            return services;
        }
    }
}