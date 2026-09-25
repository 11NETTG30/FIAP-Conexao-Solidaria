using Microsoft.Extensions.DependencyInjection;

namespace ConexaoSolidaria.IoC;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddDependencies()
        {
            services.AddDomain();
            services.AddApplication();
            services.AddInfrastructure();
        }
    }
}