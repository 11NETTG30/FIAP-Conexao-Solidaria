using FCG.Domain.Shared.Abstractions;
using Microsoft.Extensions.Logging;

namespace FCG.Infrastructure.Shared;

public sealed class DomainLogger<T> : IDomainLogger<T>
{
    private readonly ILogger<T> _logger;

    public DomainLogger(ILogger<T> logger)
    {
        _logger = logger;
    }
    
    public void LogInformation(string? message, params object?[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string? message, params object?[] args)
        => _logger.LogWarning(message, args);

    public void LogError(string? message, params object[] args)
        => _logger.LogError(message, args);

    public void LogError(Exception? exception, string? message, params object[] args)
        => _logger.LogError(exception, message, args);
}
