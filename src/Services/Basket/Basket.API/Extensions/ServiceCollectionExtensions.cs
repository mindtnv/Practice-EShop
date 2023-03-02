using Basket.API.Infrastructure.Repositories;
using Basket.API.IntegrationEvents.Consumers;
using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace Basket.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredControllers(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }

    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Basket HTTP API",
                Version = "v1",
            });
            options.AddSecurityDefinition("Bearer authentication", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer authentication",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        });

        return services;
    }

    public static IServiceCollection AddConfiguredRedisBasketRepository(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BasketSettings>(configuration.GetSection(nameof(BasketSettings)));
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<BasketSettings>>();
            var settings = options.Value;
            var configurationOptions = ConfigurationOptions.Parse(settings.RedisConnectionString);
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
        services.AddTransient<IBasketRepository, RedisBasketRepository>();
        return services;
    }

    public static IServiceCollection AddConfiguredMassTransit(this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSection = configuration.GetSection("RabbitMq");
        services.AddMassTransit(cfg =>
        {
            cfg.UsingRabbitMq((ctx, c) =>
            {
                c.Host(rabbitMqSection["Host"], h =>
                {
                    h.Username(rabbitMqSection["Username"]);
                    h.Password(rabbitMqSection["Password"]);
                });
                c.ConfigureEndpoints(ctx);
            });

            cfg.AddConsumer<CatalogItemPriceChangedConsumer>();
        });

        return services;
    }

    public static IServiceCollection AddConfiguredAuthorization(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment env)
    {
        services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    var bearerOptions = configuration.GetSection("JwtBearer");
                    options.Authority = bearerOptions["Authority"];
                    if (env.IsDevelopment())
                        options.RequireHttpsMetadata = false;
                });
        return services;
    }
}