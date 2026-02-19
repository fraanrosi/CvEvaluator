using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace CvEvaluator.Application.Interfaces;

public interface ICvEvaluationService
{
    Task<Guid> ExecuteAsync(
       IFormFile file,
       Guid userId,
       CancellationToken ct);
} 