namespace FCG.Domain.Shared.Abstractions;

public interface IDomainLogger<out TCategoryName>
{
    void LogInformation(string? message, params object?[] args);
    void LogWarning(string? message, params object?[] args);
    void LogError(string? message, params object[] args);
    void LogError(Exception? exception, string? message, params object[] args);
}
