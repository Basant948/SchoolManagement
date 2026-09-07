using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SchoolManagement.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startedAt = DateTime.UtcNow;

            await _next(context);

            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
            Console.WriteLine(
                $"[{DateTime.UtcNow:O}] {context.Request.Method} {context.Request.Path} " +
                $"=> {context.Response.StatusCode} ({elapsedMs:F0}ms)");
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}