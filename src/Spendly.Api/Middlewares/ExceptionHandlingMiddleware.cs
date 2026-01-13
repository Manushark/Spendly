using System.Net;
using System.Text.Json;
using Spendly.Domain.Exceptions;

namespace Spendly.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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
            catch (DomainException ex)
            {
                await HandleException(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception)
            {
                await HandleException(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred."
                );
            }
        }

        private static async Task HandleException(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = context.Response.StatusCode,
                error = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
