using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FCG.API.Middlewares;

public class UnauthorizedAccessExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnauthorizedAccessExceptionMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    
    public UnauthorizedAccessExceptionMiddleware
    (
        RequestDelegate next,
        ILogger<UnauthorizedAccessExceptionMiddleware> logger,
        ProblemDetailsFactory problemDetailsFactory
    )
    {
        _next = next;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            const int statusCode = StatusCodes.Status401Unauthorized;
            
            ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(context);
            problemDetails.Status = statusCode;
            problemDetails.Title = "Não autorizado";
            problemDetails.Detail = unauthorizedAccessException.Message;
            
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

public static class UnauthorizedAccessExceptionMiddlewareExtensions
{
    extension(WebApplication app)
    {
        public void UseUnauthorizedAccessExceptionMiddleware()
        {
            app.UseMiddleware<UnauthorizedAccessExceptionMiddleware>();
        }
    }
}

