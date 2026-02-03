using Microsoft.OpenApi.Models;

namespace CvEvaluator.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithApiKey(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CV Evaluator API",
                Version = "v1"
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key required. Use header: X-API-KEY",
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
