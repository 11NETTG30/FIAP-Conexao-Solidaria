using ConexaoSolidaria.Domain.Identidade.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConexaoSolidaria.IoC;

public static class DependencyInjectionDomain
{
    extension(IServiceCollection services)
    {
        internal void AddDomain()
        {
            services.AddScoped<IRefreshTokenDomainService, RefreshTokenDomainService>();
        }
    }
}