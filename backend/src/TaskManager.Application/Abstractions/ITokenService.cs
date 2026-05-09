using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(User user);
}
