using System.Globalization;
using FCG.Application.Identidade.UseCases;
using FCG.Application.Identidade.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.IoC;

public static class DependencyInjectionApplication
{
    extension(IServiceCollection services)
    {
        internal void AddApplication()
        {
            services.AddFluentValidation();
            
            services.AddScoped<AtivarUsuarioUseCase>();
            services.AddScoped<AlterarSenhaUseCase>();
            services.AddScoped<CriarUsuarioUseCase>();
            services.AddScoped<InativarUsuarioUseCase>();
            services.AddScoped<ListarTodosUsuariosUseCase>();
            services.AddScoped<LoginUseCase>();
            services.AddScoped<LogoutUseCase>();
            services.AddScoped<ObterContaUseCase>();
            services.AddScoped<ObterUsuarioPorIdUseCase>();
            services.AddScoped<RefreshTokenUseCase>();
            services.AddScoped<TornarUsuarioAdministradorUseCase>();
        }

        private void AddFluentValidation()
        {
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            services.AddFluentValidationClientsideAdapters();
            services.AddFluentValidationAutoValidation();
            
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("pt-Br");
            ValidatorOptions.Global.LanguageManager = new CustomLanguageManager();
        }
    }
    private class CustomLanguageManager : LanguageManager
    {
        private const string _PT_BR = "pt-BR";
        
        public CustomLanguageManager()
        {
            AddTranslation(_PT_BR, "MinimumLengthValidator",
                "'{PropertyName}' deve ter no mínimo {MinLength} caracteres. Você digitou {TotalLength} caracteres.");
            
            AddTranslation(_PT_BR, "MaximumLengthValidator",
                "'{PropertyName}' deve ter no máximo {MaxLength} caracteres. Você digitou {TotalLength} caracteres.");
        }
    }
}

