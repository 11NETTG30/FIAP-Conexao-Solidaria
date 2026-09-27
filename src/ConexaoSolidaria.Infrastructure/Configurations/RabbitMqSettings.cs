using System.ComponentModel.DataAnnotations;

namespace ConexaoSolidaria.Infrastructure.Configurations;

public sealed class RabbitMqSettings
{
    [Required]
    public required string Host { get; init; }

    public string VirtualHost { get; init; } = "/";

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}