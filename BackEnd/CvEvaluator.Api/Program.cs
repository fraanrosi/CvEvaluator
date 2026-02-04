using CvEvaluator.Api.Extensions;
using CvEvaluator.Infrastructure.Llm;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// =========================
// CONFIGURATION
// =========================

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    // User Secrets SOLO se usan en Development
    .AddUserSecrets<Program>(optional: true)
    // Docker / Hosting (Render, Cloud Run, etc.)
    .AddEnvironmentVariables();

// =========================
// LOGGING
// =========================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// =========================
// SERVICES
// =========================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithApiKey();

builder.Services.AddHttpClient<
    CvEvaluator.Application.Interfaces.ILlmClient,
    OllamaClient>();

builder.Services.AddScoped<
    CvEvaluator.Application.Interfaces.ICvEvaluationService,
    CvEvaluator.Application.UseCases.CvEvaluationService>();

builder.Services.AddScoped<
    CvEvaluator.Application.Interfaces.ILlmClient,
    OllamaClient>();

builder.Services.AddScoped<
    CvEvaluator.Application.Interfaces.IDocumentParser,
    CvEvaluator.Infrastructure.Parsing.PdfDocumentParser>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200"
            // En producción agregás el dominio real del frontend
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// =========================
// LOGGERS
// =========================

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(
    "API starting in {Environment}",
    app.Environment.EnvironmentName
);

// =========================
// ERROR HANDLING
// =========================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Unexpected server error");
    });
});

// =========================
// SECURITY HEADERS
// =========================

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// =========================
// HTTP REQUEST LOGGING
// =========================

app.Use(async (context, next) =>
{
    logger.LogInformation(
        "HTTP {Method} {Path}",
        context.Request.Method,
        context.Request.Path
    );
    await next();
});

// =========================
// MIDDLEWARES
// =========================

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");

app.UseMiddleware<CvEvaluator.Api.Middlewares.ApiKeyMiddleware>();

// =========================
// ENDPOINTS
// =========================

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}));

// =========================
// RUN
// =========================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");