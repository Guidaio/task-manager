using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Messaging;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Notifications;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Security;
using TaskManager.Infrastructure.SignalR;

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

        services.AddSingleton(Channel.CreateUnbounded<NotificationDispatchRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false,
        }));
        services.AddSingleton<INotificationPublisher>(sp =>
        {
            var channel = sp.GetRequiredService<Channel<NotificationDispatchRequest>>();
            return new NotificationPublisher(channel.Writer);
        });
        services.AddHostedService(sp =>
        {
            var channel = sp.GetRequiredService<Channel<NotificationDispatchRequest>>();
            return new NotificationDispatchWorker(
                channel.Reader,
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<NotificationDispatchWorker>>());
        });

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
