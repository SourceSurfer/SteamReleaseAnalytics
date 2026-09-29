using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using SteamReleaseAnalytics.Api.Auth;
using SteamReleaseAnalytics.Api.Swagger;
using SteamReleaseAnalytics.Infrastructure.Data;
using SteamReleaseAnalytics.Infrastructure.Repositories;
using SteamReleaseAnalytics.Services.Security;
using SteamReleaseAnalytics.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(AuthorizeOperationFilter.SchemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Токен из POST /api/v1/auth/login"
    });
    options.OperationFilter<AuthorizeOperationFilter>();
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SteamDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IGameSnapshotRepository, GameSnapshotRepository>();

// Register Services
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// JWT Authentication: no built-in fallback secret, the app refuses to start without a valid one
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<DemoUserOptions>()
    .BindConfiguration(DemoUserOptions.SectionName);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.Value.CreateSigningKey(),
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateLifetime = true
        };
    });

// CORS: only origins listed in Cors:AllowedOrigins
builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>()
    .Configure<IConfiguration>((cors, configuration) =>
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        cors.AddDefaultPolicy(policy => policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader());
    });

var app = builder.Build();

// Apply pending migrations on startup (enabled in launchSettings.json and docker-compose.yml)
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<SteamDbContext>().Database.Migrate();
}

// Unhandled exceptions and bare status codes become RFC 7807 problem details, without stack traces
app.UseExceptionHandler();
app.UseStatusCodePages();

// Enable Swagger for both Development and Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steam Release Analytics API V1");
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
