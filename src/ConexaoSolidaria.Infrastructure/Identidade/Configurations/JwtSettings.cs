using System.ComponentModel.DataAnnotations;
using ConexaoSolidaria.Application.Identidade.Security;

namespace ConexaoSolidaria.Infrastructure.Identidade.Configurations;

public sealed class JwtSettings : ITokenSettings
{
    [Required]
    [MinLength(32)]
    public required string Secret { get; init; }
    
    [Required]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
    
    [Range(5, 1440)]
    public short ExpiracaoAccessTokenMinutos { get; init; }
    
    [Range(1, 30)]
    public byte ExpiracaoRefreshTokenDias { get; init; }
    
    public bool HabilitarSegurancaDeReusoRefreshToken { get; init; } = false;
}