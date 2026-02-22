using CvEvaluator.Api.Extensions;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Services;
using CvEvaluator.Infrastructure.Background;
using CvEvaluator.Infrastructure.Identity;
using CvEvaluator.Infrastructure.Llm;
using CvEvaluator.Infrastructure.Parsing;
using CvEvaluator.Infrastructure.Persistence;
using CvEvaluator.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    .AddUserSecrets<Program>(optional: true)
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

builder.Services.AddDbContext<CvEvaluatorDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithApiKey();

builder.Services.AddHttpClient<ILlmClient, OllamaClient>();

builder.Services.AddScoped<ICvEvaluationRepository, CvEvaluationRepository>();
builder.Services.AddScoped<ICvEvaluationService, CvEvaluationService>();
builder.Services.AddScoped<IDocumentParser, PdfDocumentParser>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

// =========================
// IDENTITY (PRIMERO)
// =========================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<CvEvaluatorDbContext>()
    .AddDefaultTokenProviders();

// Evita redirects a login (API pura)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// =========================
// AUTHENTICATION (DESPUÉS DE IDENTITY)
// =========================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine("TOKEN VALIDATED");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("AUTH FAILED: " + context.Exception?.Message);
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

// =========================
// AUTHORIZATION (AL FINAL)
// =========================

builder.Services.AddAuthorization();

// =========================
// CORS
// =========================

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
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// =========================
// STARTUP LOG
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
// REQUEST LOGGING
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
// MIDDLEWARE ORDER
// =========================

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

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