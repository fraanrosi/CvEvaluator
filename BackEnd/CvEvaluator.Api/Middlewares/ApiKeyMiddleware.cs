using System.Net;

namespace CvEvaluator.Api.Middlewares;

public class ApiKeyMiddleware
{
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
        // ✅ Dejar pasar preflight CORS
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        if (apiKey != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }

}