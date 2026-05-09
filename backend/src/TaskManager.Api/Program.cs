using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var signingKeyBytes = Encoding.UTF8.GetBytes(jwtSection.SigningKey);
if (signingKeyBytes.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 UTF-8 bytes.");

var signingKey = new SymmetricSecurityKey(signingKeyBytes);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtSection.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSection.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync().ConfigureAwait(false);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "Task Manager API", note = "Controllers will be added in the next milestone." }));

app.Run();

#pragma warning disable CA1515 // Needed for WebApplicationFactory integration tests later
public partial class Program
{
}
#pragma warning restore CA1515
