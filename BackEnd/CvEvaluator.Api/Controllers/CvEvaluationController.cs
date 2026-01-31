using Microsoft.AspNetCore.Mvc;
using CvEvaluator.Application.UseCases;
using CvEvaluator.Application.Interfaces;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/cv")]
public class CvEvaluationController : ControllerBase
{
    private readonly ICvEvaluationService _CvEvaluationService;
    private readonly IDocumentParser _documentParser;

    public CvEvaluationController(ICvEvaluationService useCase, IDocumentParser documentParser)
    {
        _CvEvaluationService = useCase;
        _documentParser = documentParser;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] string cvText)
    {
        var (decision, result) = await _CvEvaluationService.ExecuteAsync(cvText);

        return Ok(new
        {
            decision = decision.ToString(),
            score = result.Score,
            strengths = result.Strengths,
            weaknesses = result.Weaknesses
        });
    }

    [HttpPost("evaluate-pdf")]
    public async Task<IActionResult> EvaluatePdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are supported");

        string text;

        using (var stream = file.OpenReadStream())
        {
            text = await _documentParser.ParseAsync(stream);
        }

        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Could not extract text from PDF");

        var (decision, result) = await _CvEvaluationService.ExecuteAsync(text);

        return Ok(new
        {
            decision = decision.ToString(),
            score = result.Score,
            strengths = result.Strengths,
            weaknesses = result.Weaknesses
        });
    }
}