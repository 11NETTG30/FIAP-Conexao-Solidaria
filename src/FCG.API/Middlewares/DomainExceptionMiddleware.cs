using FCG.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FCG.API.Middlewares;

public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    
    public DomainExceptionMiddleware
    (
        RequestDelegate next,
        ProblemDetailsFactory problemDetailsFactory
    )
    {
        _next = next;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            await GerarProblemDetails(
                context,
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                validationException.Message);
        }
        catch (ConflictException conflictException)
        {
            await GerarProblemDetails(
                context,
                StatusCodes.Status409Conflict,
                "Conflito",
                conflictException.Message);
        }
    }

    private async Task GerarProblemDetails(HttpContext context, int statusCode, string title, string detail)
    {
        ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(context);
        problemDetails.Status = statusCode;
        problemDetails.Title = title;
        problemDetails.Detail = detail;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public static class DomainExceptionMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        public void UseDomainExceptionMiddleware()
        {
            app.UseMiddleware<DomainExceptionMiddleware>();
        }
    }
}