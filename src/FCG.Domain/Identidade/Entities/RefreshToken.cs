using FCG.Domain.Identidade.Enums;
using FCG.Domain.Shared.Abstractions;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Domain.Identidade.Entities;

public sealed class RefreshToken : Entity, IAggregateRoot
{
    public Guid UsuarioId { get; private set; }
    public Guid Token { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiracaoEm { get; private set; }
    public bool Revogado { get; private set; }
    public DateTime? RevogadoEm { get; private set; }
    public MotivoRevogacaoRefreshToken? MotivoRevogacao { get; private set; }
    public Guid? SubstituidoPorId { get; private set; }
    
    // EF Core
    public Usuario? Usuario { get; private set; }
    public RefreshToken? SubstituidoPor { get; private set; }
    
    public RefreshToken
    (
        Guid usuarioId,
        byte expiracaoRefreshTokenDias
    )
    {
        UsuarioId = usuarioId;
        Token = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
        ExpiracaoEm = DateTime.UtcNow.AddDays(expiracaoRefreshTokenDias);
        Revogado = false;
        RevogadoEm =  null;
        SubstituidoPor = null;
    }

    // EF Core
    private RefreshToken() {}
    
    public void Revogar(MotivoRevogacaoRefreshToken motivoRevogacao)
    {
        if (Revogado)
            throw new ValidationException("O Refresh Token já está revogado");
        
        if (!Enum.IsDefined(motivoRevogacao))
            throw new ValidationException("Motivo de revogação inválido");
        
        Revogado = true;
        RevogadoEm = DateTime.UtcNow;
        MotivoRevogacao = motivoRevogacao;
    }

    public void Substituir(RefreshToken refreshTokenSubstituto)
    {
        SubstituidoPor = refreshTokenSubstituto;
        SubstituidoPorId = refreshTokenSubstituto.Id;
        Revogar(MotivoRevogacaoRefreshToken.Substituicao);
    }

    public bool ValidarTokenValido() =>
        !Revogado && DateTime.UtcNow < ExpiracaoEm;

}