using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SchoolManagement.Domain.Exceptions;

namespace SchoolManagement.Web.Middleware
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                ApiException apiEx => apiEx.StatusCode,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Unhandled exception on {context.Request.Method} {context.Request.Path}: {exception}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var isServerError = statusCode == (int)HttpStatusCode.InternalServerError;
            var payload = new
            {
                status = statusCode,
                message = isServerError ? "An unexpected error occurred. Please try again later." : exception.Message,
                traceId = context.TraceIdentifier
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}