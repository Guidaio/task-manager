using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.Application.Abstractions;

public interface ITaskService
{
    Task<Result<TaskDto>> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken);

    Task<Result<TaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<TaskDto>>> ListAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<TaskDto>> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);
}
