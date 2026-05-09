namespace TaskManager.Infrastructure.Seed;

/// <summary>
/// Fixed identifiers for repeatable demo data (documentation references these values).
/// </summary>
public static class DemoSeed
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid TaskWelcomeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid TaskSecondId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const string Email = "demo@taskmanager.local";

    public const string Password = "Demo_user_12345";

    public const string Name = "Demo User";
}
