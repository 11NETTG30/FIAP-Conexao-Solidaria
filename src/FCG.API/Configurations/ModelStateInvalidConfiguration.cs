using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FCG.API.Configurations;

public static class ModelStateInvalidConfiguration
{
    extension(IServiceCollection services)
    {
        public void ConfigureModelStateInvalid()
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = false;
                
                options.InvalidModelStateResponseFactory = context =>
                {
                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();

                    ValidationProblemDetails validationProblemDetails = problemDetailsFactory
                        .CreateValidationProblemDetails(context.HttpContext, context.ModelState);
        
                    validationProblemDetails.Status = StatusCodes.Status400BadRequest;
                    validationProblemDetails.Title = "Erro de validação";
                    validationProblemDetails.Detail = "Um ou mais erros de validação ocorreram";

                    return new BadRequestObjectResult(validationProblemDetails);
                };
            });
        }
    }
}