using System.Text;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PetHealthEcosystem.Api.Application.Interfaces;
using PetHealthEcosystem.Api.Application.Services;
using PetHealthEcosystem.Api.Data;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Infrastructure.Repositories;
using PetHealthEcosystem.Api.Middleware;
using PetHealthEcosystem.Api.Observability;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. LOGGING ESTRUTURADO (Serilog)
// =====================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =====================================================================
// 2. PERSISTÊNCIA RELACIONAL (EF Core - SQL Server / InMemory)
// =====================================================================
var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=PetHealthDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("PetsTestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(sqlConnectionString));
}

// =====================================================================
// 3. PERSISTÊNCIA NoSQL (MongoDB - Prontuários e Auditoria Clínica)
// =====================================================================
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IPetCareLogRepository, InMemoryPetCareLogRepository>();
}
else
{
    var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
    var mongoDatabaseName = builder.Configuration.GetConnectionString("MongoDatabase") ?? "PetHealthNoSql";

    try
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);
        builder.Services.AddSingleton<IMongoClient>(mongoClient);
        builder.Services.AddSingleton(mongoDatabase);
        builder.Services.AddScoped<IPetCareLogRepository, PetCareLogRepository>();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "MongoDB não disponível diretamente; ativando repositório NoSQL em memória de contingência.");
        builder.Services.AddSingleton<IPetCareLogRepository, InMemoryPetCareLogRepository>();
    }
}

// =====================================================================
// 4. INJEÇÃO DE DEPENDÊNCIA (Camadas de Domínio e Aplicação)
// =====================================================================
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetCareLogService, PetCareLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddMetrics();
builder.Services.AddSingleton<PetMetrics>();

// =====================================================================
// 5. AUTENTICAÇÃO E AUTORIZAÇÃO (JWT Bearer)
// =====================================================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "PetHealthEcosystemSuperSecretSecretKey2026Sprint4!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PetHealthEcosystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PetHealthEcosystemUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireVeterinarioRole", policy => policy.RequireRole("Veterinario", "Admin"));
});

// =====================================================================
// 6. CONTROLADORES E SWAGGER / OPENAPI COM SUPORTE JWT
// =====================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PetHealth API",
        Version = "v1",
        Description = "API Backend do Ecossistema de Saúde Animal Proativa PetHealth (Pilares Preventivo e Terapêutico)\n\nEquipe PetHealth:\n- Gabriel Costa Solano (RM 562325)\n- Leandro Guarido (RM 561760)\n- Kaiky Pereira Rodrigues (RM 564578)\nTurma: 2TDSPV"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header utilizando o esquema Bearer.\nDigite 'Bearer' [espaço] e então seu token no campo abaixo.\nExemplo: \"Bearer eyJhbGciOi...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================================
// 7. HEALTH CHECKS
// =====================================================================
var healthChecksBuilder = builder.Services.AddHealthChecks();

if (builder.Environment.IsEnvironment("Testing"))
{
    healthChecksBuilder.AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
}
else
{
    healthChecksBuilder.AddDbContextCheck<AppDbContext>(
        name: "database",
        tags: new[] { "db", "sqlserver", "ready" });
}

healthChecksBuilder.AddUrlGroup(
    new Uri("https://api.github.com"),
    name: "external-service",
    tags: new[] { "external", "ready" });

// =====================================================================
// 8. OPENTELEMETRY - TRACING E MÉTRICAS
// =====================================================================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "PetHealthEcosystem.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(PetMetrics.MeterName)
        .AddConsoleExporter());

var app = builder.Build();

// =====================================================================
// 9. PIPELINE HTTP
// =====================================================================
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<MetricsMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetHealthEcosystem API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoints de Health Check
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Auto-migração do EF Core ao iniciar (quando não estiver em Testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
            Log.Information("Migrações do banco de dados aplicadas com sucesso.");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Não foi possível executar migrações automáticas do banco na inicialização.");
    }
}

try
{
    Log.Information("Iniciando PetHealthEcosystem.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
