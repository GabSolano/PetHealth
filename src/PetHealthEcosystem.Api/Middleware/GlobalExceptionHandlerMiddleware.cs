using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PetHealthEcosystem.Api.Middleware
{
    /// <summary>
    /// Middleware global para captura consistente de exceções não tratadas
    /// retornando respostas no padrão RFC 7807 (ProblemDetails).
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Items.TryGetValue("CorrelationId", out var cid) 
                ? cid?.ToString() 
                : context.TraceIdentifier;

            _logger.LogError(exception, "[{CorrelationId}] Exceção não tratada capturada: {Message}", correlationId, exception.Message);

            var statusCode = exception switch
            {
                ArgumentException or InvalidOperationException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{(int)statusCode}"
            };

            problem.Extensions["correlationId"] = correlationId;
            problem.Extensions["timestamp"] = DateTime.UtcNow;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }

        private static string GetTitleForStatusCode(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => "Requisição Inválida",
            HttpStatusCode.NotFound => "Recurso Não Encontrado",
            HttpStatusCode.Unauthorized => "Não Autorizado",
            _ => "Ocorreu um erro interno no servidor"
        };
    }
}
