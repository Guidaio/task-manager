namespace TaskManager.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing secret (UTF-8). For HS256 this should be at least 256 bits (32 characters of ASCII) in production-oriented setups.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 120;
}
