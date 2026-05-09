namespace TaskManager.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Connection string for the application database (must include initial catalog).
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional connection string pointing at <c>master</c> (or another server catalog) used only to create the database if missing.
    /// </summary>
    public string? MasterConnectionString { get; set; }
}
