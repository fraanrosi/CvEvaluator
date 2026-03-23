using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/cvEvaluations")]
[EnableRateLimiting("general")]
public class CvEvaluationController : ControllerBase
{
    private readonly ICvEvaluationService _cvEvaluationService;
    private readonly ILogger<CvEvaluationController> _logger;

    public CvEvaluationController(
        ICvEvaluationService cvEvaluationService,
        ILogger<CvEvaluationController> logger)
    {
        _cvEvaluationService = cvEvaluationService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("evaluate-pdf")]
    public async Task<IActionResult> EvaluatePdf(
    [FromForm] List<IFormFile> files,
    [FromForm] Guid jobPositionId,
    CancellationToken ct)
    {
        if (files == null || files.Count == 0)
            return BadRequest("At least one PDF file is required");

        var responses = new List<object>();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB

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

            if (file.Length > maxFileSize)
            {
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = "File exceeds the 10 MB size limit"
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
                var evaluationId = await _cvEvaluationService.EvaluateAsync(
                    file,
                    userId,
                    jobPositionId,
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
                _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = "An error occurred processing this file."
                });
            }
        }

        return Accepted(responses);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    [FromServices] ICvEvaluationService service,
    CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await service.GetByIdAsync(id, userId, ct);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromServices] ICvEvaluationService service,
    CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await service.GetAllByUserAsync(userId, ct);

        return Ok(result);
    }
}