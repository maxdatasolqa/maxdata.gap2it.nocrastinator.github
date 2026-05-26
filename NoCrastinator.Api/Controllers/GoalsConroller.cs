using Azure.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoCrastinator.Api.Domain;
using NoCrastinator.Api.Services;
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<GoalsController> _logger;

    private readonly IGoalEvaluationService _evaluationService;

    //    HIGH RISK:
    //- reward calculation incorrect
    //- punishment applied incorrectly
     
    //  MEDIUM:
    //- invalid progress input

    //  LOW:
    //- UI formatting

    public GoalsController(AppDbContext context, ILogger<GoalsController> logger, IGoalEvaluationService goalEvaluationService)
    {
        _context = context;
        _logger = logger;
        _evaluationService = goalEvaluationService;
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var goals = await _context.Goals.ToListAsync();
        return Ok(goals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGoal(Guid id)
    {
        var userId = GetCurrentUserId();
        var goal = await _context.Goals.FindAsync(id);
        if (goal == null) return NotFound();
        if (goal.UserId != userId)
        {
            return Forbid();
        }
        
        return Ok(goal);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyGoals()
    {
        var userId = GetCurrentUserId();

        var goals = await _context.Goals
            .Where(g => g.UserId == userId)
            .ToListAsync();

        return Ok(goals);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal(Goal goal)
    {

        if (string.IsNullOrWhiteSpace(goal.Title))
            return BadRequest("Title required");

        // intentionally weak validation
        if (goal.Points < 0) // should be <= 0, but we allow 0 
            return BadRequest("Points must be positive");

        // NO validation for progressPercent

        goal.Id = Guid.NewGuid();

        var userId = GetCurrentUserId();

        // attempt to override
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // BUG: only override if empty
        if (string.IsNullOrEmpty(goal.UserId))
        {
            goal.UserId = userId;
        }

        _logger.LogInformation("Creating goal for user {UserId} with title {Title}", goal.UserId, goal.Title);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGoal(Guid id, Goal updated)
    {
        if (id != updated.Id) return BadRequest();

        if (updated.Status == GoalStatus.Completed && updated.ProgressPercent < 100)
        {
            // BUG: allow regression
            updated.Status = GoalStatus.InProgress;
        }

        var userId = GetCurrentUserId();

        if (updated.UserId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt");
            // BUG: should return Forbid but continues
        }

        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal == null) return NotFound();

        var userId = GetCurrentUserId();

        if (goal.UserId != userId)
        {
            return Forbid();
        }

        _context.Goals.Remove(goal);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid id, UpdateProgressRequest request)
    {
        var goal = await _context.Goals.FindAsync(id);

        if (goal == null)
            return NotFound();

        if (request.ProgressPercent > 100)
        {
            // BUG: log but allow
            _logger.LogWarning("Invalid progress");
        }

        var userId = GetCurrentUserId();

        if (goal.UserId != userId)
        {
            _logger.LogWarning("Unauthorized progress update");
            // BUG: allow it anyway
        }

        goal.ProgressPercent = request.ProgressPercent;
        _logger.LogInformation("Updating progress for goal {GoalId} to {Progress}", id, request.ProgressPercent);
        if (request.ProgressPercent == 100)
            goal.Status = GoalStatus.Completed;
        else if (request.ProgressPercent > 0)
            goal.Status = GoalStatus.InProgress;
        else
            goal.Status = GoalStatus.NotStarted;
        await _evaluationService.EvaluateAsync(goal);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("evaluate-all")]
    public async Task<IActionResult> EvaluateAll()
    {
        var goals = _context.Goals.ToList();

        foreach (var goal in goals)
        {
            await _evaluationService.EvaluateAsync(goal);
        }

        return Ok("Evaluation complete");
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("userId")?.Value;
    }

}
