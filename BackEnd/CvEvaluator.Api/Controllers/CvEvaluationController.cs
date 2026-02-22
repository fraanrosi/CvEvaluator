using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/cv")]
public class CvEvaluationController : ControllerBase
{
    private readonly ICvEvaluationService _cvEvaluationService;
    private readonly IDocumentParser _documentParser;

    public CvEvaluationController(
        ICvEvaluationService cvEvaluationService,
        IDocumentParser documentParser)
    {
        _cvEvaluationService = cvEvaluationService;
        _documentParser = documentParser;
    }

    [Authorize]
    [HttpPost("evaluate-pdf")]
    public async Task<IActionResult> EvaluatePdf(
    [FromForm] List<IFormFile> files,
    CancellationToken ct)
    {
        if (files == null || files.Count == 0)
            return BadRequest("At least one PDF file is required");

        var responses = new List<object>();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = "Empty file"
                });
                continue;
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = "Only PDF files are supported"
                });
                continue;
            }

            try
            {
                var evaluationId = await _cvEvaluationService.ExecuteAsync(
                    file,
                    userId,
                    ct);

                responses.Add(new
                {
                    evaluationId,
                    fileName = file.FileName,
                    status = "Processing"
                });
            }
            catch (Exception ex)
            {
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = ex.InnerException
                });
            }
        }

        return Accepted(responses);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    [FromServices] ICvEvaluationRepository repository,
    CancellationToken ct)
    {
        var evaluation = await repository.GetByIdAsync(id, ct);

        if (evaluation == null)
            return NotFound();

        return Ok(new
        {
            evaluation.Id,
            evaluation.OriginalFilename,
            evaluation.EvaluationResult,
            evaluation.Status,
            evaluation.OverallScore,
            evaluation.ErrorMessage,
            evaluation.CreatedAt,
            evaluation.EvaluatedAt
        });
    }
}