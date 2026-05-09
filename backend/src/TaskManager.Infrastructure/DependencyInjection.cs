using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Security;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
