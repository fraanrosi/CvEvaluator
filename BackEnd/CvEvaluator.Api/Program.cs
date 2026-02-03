using CvEvaluator.Api.Extensions;
using CvEvaluator.Infrastructure.Llm;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithApiKey();

builder.Services.AddHttpClient<CvEvaluator.Application.Interfaces.ILlmClient, OllamaClient>();


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
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("FrontendPolicy");

// Middleware
app.UseMiddleware<CvEvaluator.Api.Middlewares.ApiKeyMiddleware>();

app.MapControllers();

app.Run();