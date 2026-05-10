using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Messaging;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.UnitTests;

public sealed class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<INotificationPublisher> _notifications = new();
    private readonly Mock<INotificationRepository> _notificationRepo = new();

    private TaskService CreateSut()
    {
        _notifications
            .Setup(x => x.PublishAsync(It.IsAny<NotificationDispatchRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _notificationRepo
            .Setup(x => x.DetachTaskReferencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new TaskService(_tasks.Object, _notifications.Object, _notificationRepo.Object);
    }

    private static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Create_ShouldFail_WhenTitleIsEmpty()
    {
        var sut = CreateSut();

        var result = await sut.CreateAsync(UserId, new CreateTaskRequest { Title = "   ", Status = TaskItemStatus.Pending }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Title is required.", result.Error);
        _tasks.Verify(x => x.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldFail_WhenStatusIsInvalid()
    {
        var sut = CreateSut();
        var invalidStatus = (TaskItemStatus)999;

        var result = await sut.CreateAsync(UserId, new CreateTaskRequest { Title = "Ok", Status = invalidStatus }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid task status.", result.Error);
        _tasks.Verify(x => x.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldCreateTask_WhenRequestIsValid()
    {
        var sut = CreateSut();
        TaskItem? captured = null;
        _tasks.Setup(x => x.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns<TaskItem, CancellationToken>((t, _) =>
            {
                captured = t;
                return Task.FromResult(t);
            });

        var due = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await sut.CreateAsync(
            UserId,
            new CreateTaskRequest { Title = " My task ", Description = " Desc ", Status = TaskItemStatus.InProgress, DueDateUtc = due },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("My task", result.Value!.Title);
        Assert.Equal("Desc", result.Value.Description);
        Assert.Equal(TaskItemStatus.InProgress, result.Value.Status);
        Assert.Equal(due, result.Value.DueDateUtc);
        Assert.NotNull(captured);
        Assert.Equal(UserId, captured!.UserId);
    }

    [Fact]
    public async Task List_ReturnsPagedDtos_FromRepository()
    {
        var sut = CreateSut();
        var task = new TaskItem
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserId = UserId,
            Title = "One",
            Description = null,
            Status = TaskItemStatus.Pending,
            DueDateUtc = null,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        _tasks
            .Setup(x => x.ListByUserIdPagedAsync(UserId, null, 1, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([task], 1));

        var result = await sut.ListAsync(UserId, null, 1, 25, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(25, result.Value.PageSize);
    }

    [Fact]
    public async Task Update_ShouldFail_WhenTaskDoesNotBelongToUser()
    {
        var sut = CreateSut();
        var taskId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        _tasks.Setup(x => x.GetByIdAndUserIdAsync(taskId, UserId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        var result = await sut.UpdateAsync(
            UserId,
            taskId,
            new UpdateTaskRequest { Title = "Updated", Status = TaskItemStatus.Completed },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Task was not found.", result.Error);
        _tasks.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ShouldFail_WhenTaskDoesNotExist()
    {
        var sut = CreateSut();
        var taskId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        _tasks.Setup(x => x.GetByIdAndUserIdAsync(taskId, UserId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        var result = await sut.DeleteAsync(UserId, taskId, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Task was not found.", result.Error);
        _tasks.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
