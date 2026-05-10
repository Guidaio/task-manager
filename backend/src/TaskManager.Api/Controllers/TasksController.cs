using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Extensions;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserIdOrUnauthorized();
        if (userId is null)
            return UnauthorizedUser();

        TaskItemStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TaskItemStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
                return BadRequest(new { error = "Invalid status. Use Pending, InProgress, Completed, or Cancelled." });

            statusFilter = parsed;
        }

        var p = page is null || page.Value < 1 ? 1 : page.Value;
        var ps = pageSize is null || pageSize.Value < 1 ? 25 : pageSize.Value;
        if (ps > 100)
            ps = 100;

        var result = await _taskService.ListAsync(userId.Value, statusFilter, p, ps, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = ResolveUserIdOrUnauthorized();
        if (userId is null)
            return UnauthorizedUser();

        var result = await _taskService.GetByIdAsync(userId.Value, id, cancellationToken);
        return MapTaskResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = ResolveUserIdOrUnauthorized();
        if (userId is null)
            return UnauthorizedUser();

        var result = await _taskService.CreateAsync(userId.Value, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = ResolveUserIdOrUnauthorized();
        if (userId is null)
            return UnauthorizedUser();

        var result = await _taskService.UpdateAsync(userId.Value, id, request, cancellationToken);
        return MapTaskResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = ResolveUserIdOrUnauthorized();
        if (userId is null)
            return UnauthorizedUser();

        var result = await _taskService.DeleteAsync(userId.Value, id, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.Error == "Task was not found.")
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    private Guid? ResolveUserIdOrUnauthorized() => User.TryGetUserId();

    private static IActionResult UnauthorizedUser() =>
        new UnauthorizedObjectResult(new { error = "Missing or invalid authentication." });

    private IActionResult MapTaskResult(Result<TaskDto> result)
    {
        if (result.Succeeded)
            return Ok(result.Value);

        if (result.Error == "Task was not found.")
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}
