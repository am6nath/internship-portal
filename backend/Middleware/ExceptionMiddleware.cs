using System.Net;
using System.Text.Json;

namespace InternshipPortal.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(
                    context,
                    ex);
            }
        }

        private static async Task
            HandleExceptionAsync(
                HttpContext context,
                Exception exception)
        {
            context.Response.ContentType =
                "application/json";

            context.Response.StatusCode = exception is UnauthorizedAccessException
                ? (int)HttpStatusCode.Unauthorized
                : (int)HttpStatusCode.BadRequest;

            var response = new
            {
                success = false,
                statusCode =
                    context.Response.StatusCode,

                message = exception.Message
            };

            var json =
                JsonSerializer.Serialize(response);

            await context.Response
                .WriteAsync(json);
        }
    }
}

