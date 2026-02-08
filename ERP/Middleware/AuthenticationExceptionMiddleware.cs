using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ERP.Middleware
{
    /// <summary>
    /// Middleware ???? ?????? ?????? Authentication ? redirect ?? ???? ?????
    /// </summary>
    public class AuthenticationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationExceptionMiddleware> _logger;

        public AuthenticationExceptionMiddleware(RequestDelegate next, ILogger<AuthenticationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // ??? response ?? 401 Unauthorized ????
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    _logger.LogWarning($"Unauthorized request: {context.Request.Path}");
                    
                    // ???? AJAX requests? JSON response ????? ??
                    if (context.Request.IsAjaxRequest())
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new 
                        { 
                            success = false, 
                            message = "???? ???? ????? ??? ???. ????? ?????? ???? ????.",
                            redirectUrl = "/Identity/Account/Login" // ???? ???? ?????
                        });
                    }
                    else
                    {
                        // ???? regular requests? redirect ??
                        context.Response.Redirect("/Identity/Account/Login");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access exception");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                
                if (context.Request.IsAjaxRequest())
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        success = false, 
                        message = "?????? ???????. ????? ?????? ???? ????.",
                        redirectUrl = "/Identity/Account/Login"
                    });
                }
                else
                {
                    context.Response.Redirect("/Identity/Account/Login");
                }
            }
        }
    }

    /// <summary>
    /// Extension method ???? ????? AJAX requests
    /// </summary>
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}
