using System.Net;
using System.Text.Json;
using Spendly.Domain.Exceptions;

namespace Spendly.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ExpenseNotFoundException ex)
            {
                await HandleException(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (UnauthorizedExpenseAccessException ex)
            {
                // 403 Forbidden: el recurso existe pero no pertenece al usuario
                await HandleException(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (InvalidCredentialsException ex)
            {
                await HandleException(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (DomainException ex)
            {
                await HandleException(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleException(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static async Task HandleException(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { status = context.Response.StatusCode, error = message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}