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

    public async Task InvokeAsync(HttpContext context)
    {
        // Ignore all routes under /api/auth
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context); // Skip token validation for authentication routes
            return;
        }

        // Extract the token from the Authorization header
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // Check if the token is null or invalid
        if (string.IsNullOrEmpty(token) )
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Message = "Oops! Something went wrong, please call support."
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            return; // Return early to avoid calling the next middleware
        }

        // Call the next middleware in the pipeline if token is valid
        await _next(context);
    }

    // private bool IsValidToken(string token)
    // {
    //     // Replace with actual token validation logic
        
    //     return true; // Replace with actual validation
    // }
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
