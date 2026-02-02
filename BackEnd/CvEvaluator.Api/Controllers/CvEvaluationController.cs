using Microsoft.AspNetCore.Mvc;
using CvEvaluator.Application.Interfaces;

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

    [HttpPost("evaluate-pdf")]
    public async Task<IActionResult> EvaluatePdf(
        [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("At least one PDF file is required");

        var responses = new List<object>();

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

            string text;

            using (var stream = file.OpenReadStream())
            {
                text = await _documentParser.ParseAsync(stream);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                responses.Add(new
                {
                    fileName = file.FileName,
                    error = "Could not extract text from PDF"
                });
                continue;
            }

            var (decision, result) = await _cvEvaluationService.ExecuteAsync(text);

            responses.Add(new
            {
                fileName = file.FileName,
                decision = decision.ToString(),
                score = result.Score,
                strengths = result.Strengths,
                weaknesses = result.Weaknesses
            });
        }

        return Ok(responses);
    }
}