using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Infrastructure.Doacoes.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConexaoSolidaria.Infrastructure.Configurations;

public static class MessagingConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddMessaging
        (
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator>? configurarOutbox = null,
            Action<IBusRegistrationConfigurator>? registrarConsumidores = null,
            Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configurarEndpoints = null
        )
        {
            IConfigurationSection rabbitMqSettings = configuration.GetSection("RabbitMqSettings");

            services.AddOptions<RabbitMqSettings>()
                .Bind(rabbitMqSettings)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddMassTransit(busConfigurator =>
            {
                configurarOutbox?.Invoke(busConfigurator);
                registrarConsumidores?.Invoke(busConfigurator);

                busConfigurator.UsingRabbitMq((context, rabbitConfigurator) =>
                {
                    RabbitMqSettings settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                    rabbitConfigurator.Host(settings.Host, settings.VirtualHost, host =>
                    {
                        host.Username(settings.Username);
                        host.Password(settings.Password);
                    });

                    if (configurarEndpoints is not null)
                        configurarEndpoints(context, rabbitConfigurator);
                    else
                        rabbitConfigurator.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        }
    }
}
