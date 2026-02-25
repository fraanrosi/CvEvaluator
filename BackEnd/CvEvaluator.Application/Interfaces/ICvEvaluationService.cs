using CvEvaluator.Application.DTOs;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace CvEvaluator.Application.Interfaces;

public interface ICvEvaluationService
{
    Task<Guid> EvaluateAsync(
       IFormFile file,
       Guid userId,
       CancellationToken ct);
    Task<CvEvaluationDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
    Task<IEnumerable<CvEvaluationDto>> GetAllByUserAsync(Guid userId, CancellationToken ct);
}