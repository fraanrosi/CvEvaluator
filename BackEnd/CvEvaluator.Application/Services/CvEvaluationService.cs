using CvEvaluator.Application.Interfaces;
using CvEvaluator.Domain.Entities;
using CvEvaluator.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

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

    public async Task<Guid> ExecuteAsync(
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
        
        //var user = new User("email", "user", "1234", 0);

        var evaluation = new CvEvaluation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse("ff520065-f03c-46c3-b5b0-d27beb1559d1"),
            //User = user,
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
}
