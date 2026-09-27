using ConexaoSolidaria.Application.Shared;
using MassTransit;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Messaging;

// Implementação da abstração de publicação da Application usando
// MassTransit/RabbitMQ — a Application não conhece o broker.
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Publicar<T>(T evento, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(evento, cancellationToken);
    }
}
