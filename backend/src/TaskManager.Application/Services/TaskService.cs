using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Messaging;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly INotificationPublisher _notifications;
    private readonly INotificationRepository _notificationRepository;

    public TaskService(
        ITaskRepository tasks,
        INotificationPublisher notifications,
        INotificationRepository notificationRepository)
    {
        _tasks = tasks;
        _notifications = notifications;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<TaskDto>> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty)
            return Result<TaskDto>.Fail("User is required.");

        var validation = ValidateTaskWrite(request.Title, request.Status);
        if (validation is not null)
            return Result<TaskDto>.Fail(validation);

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            DueDateUtc = request.DueDateUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var created = await _tasks.CreateAsync(task, cancellationToken).ConfigureAwait(false);
        await _notifications
            .PublishAsync(
                new NotificationDispatchRequest(
                    userId,
                    created.Id,
                    $"Task \"{created.Title}\" was created.",
                    NotificationType.Success),
                cancellationToken)
            .ConfigureAwait(false);
        return Result<TaskDto>.Ok(ToDto(created));
    }

    public async Task<Result<TaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            return Result<TaskDto>.Fail("User is required.");

        if (taskId == Guid.Empty)
            return Result<TaskDto>.Fail("Task id is required.");

        var task = await _tasks.GetByIdAndUserIdAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (task is null)
            return Result<TaskDto>.Fail("Task was not found.");

        return Result<TaskDto>.Ok(ToDto(task));
    }

    public async Task<Result<TaskListResponseDto>> ListAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        TaskListSortBy sortBy,
        bool descending,
        string? search,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            return Result<TaskListResponseDto>.Fail("User is required.");

        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 25;
        if (pageSize > 100)
            pageSize = 100;

        string? normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (normalizedSearch is { Length: > 200 })
            return Result<TaskListResponseDto>.Fail("Search text must be 200 characters or fewer.");

        var (items, total) = await _tasks
            .ListByUserIdPagedAsync(userId, status, page, pageSize, sortBy, descending, normalizedSearch, cancellationToken)
            .ConfigureAwait(false);
        var dtos = items.Select(ToDto).ToArray();
        return Result<TaskListResponseDto>.Ok(
            new TaskListResponseDto
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            });
    }

    public async Task<Result<TaskDto>> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty)
            return Result<TaskDto>.Fail("User is required.");

        if (taskId == Guid.Empty)
            return Result<TaskDto>.Fail("Task id is required.");

        var validation = ValidateTaskWrite(request.Title, request.Status);
        if (validation is not null)
            return Result<TaskDto>.Fail(validation);

        var existing = await _tasks.GetByIdAndUserIdAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (existing is null)
            return Result<TaskDto>.Fail("Task was not found.");

        existing.Title = request.Title.Trim();
        existing.Description = request.Description?.Trim();
        existing.Status = request.Status;
        existing.DueDateUtc = request.DueDateUtc;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _tasks.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        if (!updated)
            return Result<TaskDto>.Fail("Task could not be updated.");

        await _notifications
            .PublishAsync(
                new NotificationDispatchRequest(
                    userId,
                    existing.Id,
                    $"Task \"{existing.Title}\" was updated.",
                    NotificationType.Info),
                cancellationToken)
            .ConfigureAwait(false);

        return Result<TaskDto>.Ok(ToDto(existing));
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            return Result.Fail("User is required.");

        if (taskId == Guid.Empty)
            return Result.Fail("Task id is required.");

        var existing = await _tasks.GetByIdAndUserIdAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (existing is null)
            return Result.Fail("Task was not found.");

        var title = existing.Title;
        await _notificationRepository
            .DetachTaskReferencesAsync(taskId, cancellationToken)
            .ConfigureAwait(false);
        await _notifications
            .PublishAsync(
                new NotificationDispatchRequest(
                    userId,
                    null,
                    $"Task \"{title}\" was deleted.",
                    NotificationType.Warning),
                cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _tasks.DeleteAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (!deleted)
            return Result.Fail("Task could not be deleted.");

        return Result.Ok();
    }

    private static string? ValidateTaskWrite(string title, TaskItemStatus status)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title is required.";

        if (!Enum.IsDefined(status))
            return "Invalid task status.";

        return null;
    }

    private static TaskDto ToDto(TaskItem task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDateUtc = task.DueDateUtc,
            CreatedAtUtc = task.CreatedAtUtc,
            UpdatedAtUtc = task.UpdatedAtUtc,
        };
}
