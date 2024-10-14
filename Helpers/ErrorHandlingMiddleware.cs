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
