using System.Reflection;
using Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Catalog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection CustomConfigure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CatalogSettings>(configuration.GetSection(nameof(CatalogSettings)));
        return services;
    }

    public static IServiceCollection AddCustomMvc(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", x =>
            {
                x.SetIsOriginAllowed(_ => true)
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogContext>(o =>
        {
            o.UseNpgsql(configuration.GetConnectionString("Default"),
                x => x.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name));
        });

        services.AddHostedService<MigrationsHostedService<CatalogContext>>();
        return services;
    }

    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EShop - Catalog.API",
                Version = "v1",
            });
        });

        return services;
    }
}