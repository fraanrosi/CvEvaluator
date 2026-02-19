using CvEvaluator.Api.Extensions;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using CvEvaluator.Infrastructure.Background;
using CvEvaluator.Infrastructure.Llm;
using CvEvaluator.Infrastructure.Parsing;
using CvEvaluator.Infrastructure.Persistence;
using CvEvaluator.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

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
    // User Secrets SOLO se usan en Development (secrets.json)
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

builder.Services.AddHostedService<CvEvaluationWorker>();
builder.Services.AddDbContext<CvEvaluatorDbContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithApiKey();
builder.Services.AddHttpClient<ILlmClient, OllamaClient>();
builder.Services.AddScoped<ICvEvaluationRepository, CvEvaluationRepository>();
builder.Services.AddScoped<ICvEvaluationService, CvEvaluationService>();
builder.Services.AddScoped<IDocumentParser, PdfDocumentParser>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // fallback seguro
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");

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