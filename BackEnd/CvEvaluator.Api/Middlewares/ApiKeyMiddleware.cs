using System.Net;

namespace CvEvaluator.Api.Middlewares;

public class ApiKeyMiddleware
{
    private const string API_KEY_HEADER = "X-API-KEY";
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["Security:ApiKey"]
                  ?? throw new ArgumentNullException("API Key not configured");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 🔓 Permitir Swagger sin API Key
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        if (!string.Equals(extractedApiKey, _apiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}