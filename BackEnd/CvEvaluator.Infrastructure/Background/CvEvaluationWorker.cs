using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Prompts;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;
using CvEvaluator.Infrastructure.Persistence;
using CvEvaluator.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections;
using System.Text.Json;

namespace CvEvaluator.Infrastructure.Background;

public class CvEvaluationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<EvaluationHub> _hubContext;
    private readonly EvaluationQueue _queue;
    public CvEvaluationWorker(
        IServiceScopeFactory scopeFactory,
        IHubContext<EvaluationHub> hubContext,
        EvaluationQueue queue)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evaluationId in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<CvEvaluatorDbContext>();
                var llm = scope.ServiceProvider.GetRequiredService<ILlmClient>();

                var evaluation = await db.Evaluations
                    .FirstOrDefaultAsync(e => e.Id == evaluationId, stoppingToken);

                if (evaluation == null)
                    continue;

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

                await db.SaveChangesAsync(stoppingToken);

                // 🔥 Notificar después de persistir
                await _hubContext
                    .Clients
                    .User(evaluation.UserId.ToString())
                    .SendAsync("EvaluationUpdated", new
                    {
                        id = evaluation.Id,
                        status = evaluation.Status.ToString(),
                        overallScore = evaluation.OverallScore,
                        errorMessage = evaluation.ErrorMessage
                    }, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker error: {ex.Message}");
            }
        }
    }
}