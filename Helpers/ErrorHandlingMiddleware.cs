// Middleware/TokenValidationMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check for Authorization header
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401 Unauthorized
            await context.Response.WriteAsync("Authorization header missing.");
            return;
        }

        await _next(context);
    }
}


public class GlobalErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlerMiddleware> _logger;

    public GlobalErrorHandlerMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlerMiddleware> logger)
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
            // Log the detailed error for internal review
            _logger.LogError(ex, "An unexpected error occurred!");

            // Return a generic 500 error response to the user
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                Message = "Oops! Something went wrong, please call support."
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
