using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CvEvaluator.Application.Services;

public class CvEvaluationService : ICvEvaluationService
{
    private readonly IDocumentParser _documentParser;
    private readonly ICvEvaluationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CvEvaluationService(
        IDocumentParser documentParser,
        ICvEvaluationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _documentParser = documentParser;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> EvaluateAsync(
        IFormFile file,
        Guid userId,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        string extractedText;

        await using (var stream = file.OpenReadStream())
        {
            extractedText = await _documentParser.ParseAsync(stream);
        }

        if (string.IsNullOrWhiteSpace(extractedText))
            throw new InvalidOperationException("Could not extract text from PDF");

        var fileHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(extractedText)));

        var evaluation = new CvEvaluation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OriginalFilename = file.FileName,
            FileSizeBytes = file.Length,
            FileHash = fileHash,
            ExtractedText = extractedText,
            Status = EvaluationStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(evaluation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return evaluation.Id;
    }

    public async Task<CvEvaluationDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var evaluation = await _repository.GetByIdAsync(id, ct);

        if (evaluation == null || evaluation.UserId != userId)
            return null;

        return MapToDto(evaluation);
    }

    public async Task<IEnumerable<CvEvaluationDto>> GetAllByUserAsync(Guid userId, CancellationToken ct)
    {
        var evaluations = await _repository.GetAllByUserIdAsync(userId, ct);

        return evaluations.Select(MapToDto);
    }

    private static CvEvaluationDto MapToDto(CvEvaluation e)
    {
        return new CvEvaluationDto
        {
            Id = e.Id,
            OriginalFilename = e.OriginalFilename,
            Status = e.Status,
            OverallScore = e.OverallScore,
            ErrorMessage = e.ErrorMessage,
            CreatedAt = e.CreatedAt,
            EvaluatedAt = e.EvaluatedAt
        };
    }
}