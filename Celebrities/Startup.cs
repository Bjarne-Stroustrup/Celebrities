using Celebrities.Database;
using Celebrities.Extensions;
using Celebrities.ViewModels.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Celebrities
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(o => o.SuppressConsumesConstraintForFormFileParameters = true)
                .AddFluentValidation(fv =>
                fv.RegisterValidatorsFromAssemblyContaining<CelebrityValidator>());

            services.AddDbContext<CelebritiesDbContext>(options =>
            {
                var dbConnectionString = Configuration.GetConnectionString("Celebrities");
                options.UseSqlServer(dbConnectionString);
            });

            services.AddFaceRecognitionServiceClient(Configuration);

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Celebrities API V1");
            });
        }
    }
}