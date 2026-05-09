using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User> CreateAsync(User user, CancellationToken cancellationToken);
}
