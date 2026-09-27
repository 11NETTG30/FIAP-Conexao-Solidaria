using ConexaoSolidaria.Application.Identidade.Security;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Campanhas.Repositories;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Shared.Abstractions;
using ConexaoSolidaria.Infrastructure.Campanhas.Persistence.Repositories;
using ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Repositories;
using ConexaoSolidaria.Infrastructure.Identidade.Configurations;
using ConexaoSolidaria.Infrastructure.Identidade.Persistence.Repositories;
using ConexaoSolidaria.Infrastructure.Identidade.Security;
using ConexaoSolidaria.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConexaoSolidaria.IoC;

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
            services.AddScoped<ICampanhaRepository, CampanhaRepository>();
            services.AddScoped<IDoacaoRepository, DoacaoRepository>();
        }
    }
}