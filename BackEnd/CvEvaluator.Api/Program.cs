using CvEvaluator.Application.UseCases;
using CvEvaluator.Infrastructure.Llm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<OllamaClient>();
builder.Services.AddScoped<CvEvaluator.Application.Interfaces.ICvEvaluationService, CvEvaluator.Application.UseCases.CvEvaluationService>();
builder.Services.AddScoped<CvEvaluator.Application.Interfaces.ILlmClient, OllamaClient>();
builder.Services.AddScoped<CvEvaluator.Application.Interfaces.IDocumentParser, CvEvaluator.Infrastructure.Parsing.PdfDocumentParser>();

var app = builder.Build();

// 🔹 Swagger (solo en dev, pero para ahora lo dejamos siempre)
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
