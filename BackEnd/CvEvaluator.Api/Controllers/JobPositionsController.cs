using CvEvaluator.Application.DTOs;
using CvEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CvEvaluator.Api.Controllers;

[ApiController]
[Route("api/jobpositions")]
[Authorize]
public class JobPositionsController : ControllerBase
{
    private readonly IJobPositionService _jobPositionService;

    public JobPositionsController(IJobPositionService jobPositionService)
    {
        _jobPositionService = jobPositionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _jobPositionService.GetAllAsync(userId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _jobPositionService.GetByIdAsync(id, userId, ct);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobPositionDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _jobPositionService.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobPositionDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var updated = await _jobPositionService.UpdateAsync(id, dto, userId, ct);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var deleted = await _jobPositionService.DeleteAsync(id, userId, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
