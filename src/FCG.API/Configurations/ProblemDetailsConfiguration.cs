namespace FCG.API.Configurations;

public static class ProblemDetailsConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddProblemDetailsConfiguration()
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path.Value;

                    ILogger<Program> logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    
                    string path = context.HttpContext.Request.Path;
                    string method = context.HttpContext.Request.Method;

                    switch (context.HttpContext.Response.StatusCode)
                    {
                        case StatusCodes.Status401Unauthorized:
                            context.ProblemDetails.Title = "Não autorizado";
                            context.ProblemDetails.Detail = "A autenticação é necessário e falhou ou não foi fornecida";
                            logger.LogWarning("401 Unauthorized - Path: {Path}, Method: {Method}", path, method);
                            break;
                        case StatusCodes.Status403Forbidden:
                            context.ProblemDetails.Title = "Acesso negado";
                            context.ProblemDetails.Detail = "Você não tem permissão para acessar este recurso";
                            logger.LogWarning("403 Forbidden - Path: {Path}, Method: {Method}", path, method);
                            break;
                        case StatusCodes.Status404NotFound:
                            context.ProblemDetails.Title = "Recurso não encontrado";
                            context.ProblemDetails.Detail = "O recurso solicitado não existe no servidor";
                            logger.LogWarning("404 NotFound - Path: {Path}, Method: {Method}", path, method);
                            break;
                        case StatusCodes.Status405MethodNotAllowed:
                            context.ProblemDetails.Title = "Método não permitido";
                            context.ProblemDetails.Detail = $"O método HTTP '{context.HttpContext.Request.Method}' usado para esta rota não é permitido";
                            logger.LogWarning("405 MethodNotAllowed - Path: {Path}, Method: {Method}", path, method);
                            break;
                        case StatusCodes.Status415UnsupportedMediaType:
                            string contentType = context.HttpContext.Request.ContentType ?? "n/a";
                            context.ProblemDetails.Title = "Tipo de mídia não suportado";
                            context.ProblemDetails.Detail = $"O tipo da mídia da requisição (ContentType = {contentType}) não é suportado pelo servidor para este recurso";
                            logger.LogWarning("415 UnsupportedMediaType - Path: {Path}, Method: {Method}, ContentType: {ContentType}", path, method, contentType);
                            break;
                    }
                };
            });
        }
    }
}