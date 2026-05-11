using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public interface ITaskService
{
    Task<Result<TaskDto>> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken);

    Task<Result<TaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);

    Task<Result<TaskListResponseDto>> ListAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        TaskListSortBy sortBy,
        bool descending,
        string? search,
        CancellationToken cancellationToken);

    Task<Result<TaskDto>> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);
}
