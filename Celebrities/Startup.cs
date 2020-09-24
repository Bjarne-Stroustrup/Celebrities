using Celebrities.Builders;
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
            services.AddCors();

            services.AddControllers()
                .AddFluentValidation(fv =>
                    fv.RegisterValidatorsFromAssemblyContaining<CelebrityValidator>());

            services.AddJwtAuthentication(Configuration);

            services.AddDbContext<CelebritiesDbContext>(options =>
            {
                var dbConnectionString = Configuration.GetConnectionString("Celebrities");
                options.UseSqlServer(dbConnectionString).UseLazyLoadingProxies();
            });

            services.AddFaceRecognitionServiceClient(Configuration);

            services.AddTransient<CelebrityBuilder, CelebrityBuilder>();
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

            app.UseCors(builder => builder.AllowAnyOrigin());

            app.UseAuthentication();
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