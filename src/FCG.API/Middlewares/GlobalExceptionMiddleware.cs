using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FCG.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    
    public GlobalExceptionMiddleware
    (
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "500 InternalServerError - {Mensagem}", ex.Message);
            
            const int statusCode = StatusCodes.Status500InternalServerError;
            
            ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(context);
            problemDetails.Status = statusCode;
            problemDetails.Title = "Erro interno no servidor";
            problemDetails.Detail = "Ocorreu um erro inesperado";
            
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    extension(WebApplication app)
    {
        public void UseGlobalExceptionMiddleware()
        {
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}

