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

app.UseCors("FrontendPolicy");
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
