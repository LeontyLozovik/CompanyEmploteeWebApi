using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Entities;
using Microsoft.EntityFrameworkCore;
using Contracts;
using Repository;
using Microsoft.Extensions.Logging;
using Marvin.Cache.Headers;

namespace WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddConfiguredCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        });

        public static void AddConfiguratedIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options => { });

        public static void AddDataBase(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("SQLConnection"), b => 
            b.MigrationsAssembly("WebApi")));

        public static void AddConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static IMvcBuilder AddCustumCsvFormater(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services)
        {
            services.AddHttpCacheHeaders(
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 65; 
                    expirationOpt.CacheLocation = CacheLocation.Private;
                },
                (validationOpt) =>
                { 
                    validationOpt.MustRevalidate = true;
                });
        }
    }
}
