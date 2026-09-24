using FCG.Application.Identidade.Security;
using FCG.Application.Shared;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Security;
using FCG.Domain.Shared.Abstractions;
using FCG.Infrastructure.Identidade.Configurations;
using FCG.Infrastructure.Identidade.Persistence.Repositories;
using FCG.Infrastructure.Identidade.Security;
using FCG.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FCG.IoC;

public static class DependencyInjectionInfrastructure
{
    extension(IServiceCollection services)
    {
        internal void AddInfrastructure()
        {
            services.AddRepositories();

            services.AddSingleton(typeof(IDomainLogger<>), typeof(DomainLogger<>));
            services.AddScoped<IInformacoesUsuarioLogado, InformacoesUsuarioLogado>();
            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<ISenhaHasher, Argon2IdSenhaHasher>();
            
            services.AddSingleton<ITokenSettings>(provider =>
            {
                JwtSettings jwtSettings = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
                return jwtSettings;
            });
        }

        private void AddRepositories()
        {
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        }
        
        
    }
}