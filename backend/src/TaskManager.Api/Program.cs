using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Api.Middleware;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
        if (origins is { Length: > 0 })
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { status = "Task Manager API", docs = "See README for HTTP routes." }))
    .AllowAnonymous();

app.Run();

#pragma warning disable CA1515 // Needed for WebApplicationFactory integration tests later
public partial class Program
{
}
#pragma warning restore CA1515
