using Microsoft.Extensions.DependencyInjection;

namespace FCG.IoC;

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