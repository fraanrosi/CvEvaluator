using System.Text.Json;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Prompts;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;
using CvEvaluator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CvEvaluator.Infrastructure.Background;

public class CvEvaluationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CvEvaluationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<CvEvaluatorDbContext>();
                var llm = scope.ServiceProvider.GetRequiredService<ILlmClient>();

                var pending = await db.Evaluations
                    .Where(e => e.Status == EvaluationStatus.Processing)
                    .ToListAsync(stoppingToken);

                foreach (var evaluation in pending)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var prompt = CvEvaluationPrompt.Build(evaluation.ExtractedText);

                        var llmResponse = await llm.EvaluateCvAsync(prompt, stoppingToken);

                        if (string.IsNullOrWhiteSpace(llmResponse))
                            throw new Exception("LLM returned empty response");

                        var result = JsonSerializer.Deserialize<CvEvaluationResult>(
                            llmResponse,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        if (result == null)
                            throw new Exception("Invalid LLM response format");

                        evaluation.EvaluationResult = llmResponse;
                        evaluation.OverallScore = result.Score;
                        evaluation.Status = EvaluationStatus.Completed;
                        evaluation.EvaluatedAt = DateTime.UtcNow;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        evaluation.Status = EvaluationStatus.Failed;
                        evaluation.ErrorMessage = ex.Message;
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}