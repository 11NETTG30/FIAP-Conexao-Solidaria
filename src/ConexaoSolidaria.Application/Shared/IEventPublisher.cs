namespace ConexaoSolidaria.Application.Shared;

// Abstração para publicação de eventos assíncronos (RabbitMQ/MassTransit na
// implementação de Infrastructure) — a Application não conhece o broker.
public interface IEventPublisher
{
    Task Publicar<T>(T evento, CancellationToken cancellationToken = default) where T : class;
}