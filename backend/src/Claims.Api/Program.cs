using System.Text;
using System.Text.Json.Serialization;
using Claims.Api.Auth;
using Claims.Api.Middleware;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Infrastructure;
using Claims.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.Section));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Section));

var adminKey = builder.Configuration[$"{AdminOptions.Section}:ApiKey"];
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(adminKey) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Falta configurar Admin:ApiKey y Jwt:Key (mínimo 32 caracteres). Revisa el README.");
}

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication()
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, _ => { })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p
        .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName, JwtBearerDefaults.AuthenticationScheme)
        .RequireRole("Admin"));

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Reclamos", Version = "v1" });
    o.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = ApiKeyAuthenticationHandler.HeaderName
    });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", doc)] = []
    });
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await InitializeDatabaseAsync(app);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            await DbSeeder.SeedAsync(db, TimeProvider.System);
            return;
        }
        catch (Exception ex) when (attempt < 10)
        {
            logger.LogWarning(ex, "Base de datos no disponible (intento {Attempt}/10), reintentando...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

public partial class Program;
