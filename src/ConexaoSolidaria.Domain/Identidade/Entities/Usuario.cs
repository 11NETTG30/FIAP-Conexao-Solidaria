using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Abstractions;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Domain.Identidade.Entities;

public sealed class Usuario : Entity, IAggregateRoot, IAuditavel
{
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; private set; }
    public SenhaHash SenhaHash { get; private set; }
    public PerfilUsuario Perfil { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public Usuario
    (
        string nome,
        Email email,
        Cpf cpf,
        SenhaHash senhaHash,
        PerfilUsuario perfil
    )
    {
        SetNome(nome);
        SetEmail(email);
        SetCpf(cpf);
        SetSenhaHash(senhaHash);
        SetPerfil(perfil);
        SetAtivo(true);
    }
    
    // EF Core
    private Usuario(){}

    public void SetNome(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            throw new ValidationException("Nome não pode ser vazio ou nulo");
        
        if (nome.Length is < 2 or > 100)
            throw new ValidationException("Nome deve ter entre 2 e 100 caracteres");
        
        Nome =  nome.Trim();
    }
    
    public void SetEmail(Email email) =>
        Email = email ?? throw new ValidationException("E-mail é obrigatório");

    public void SetCpf(Cpf cpf) =>
        Cpf = cpf ?? throw new ValidationException("CPF é obrigatório");

    public void SetSenhaHash(SenhaHash senhaHash) =>
        SenhaHash = senhaHash ?? throw new ValidationException("Senha é obrigatória");

    public void SetPerfil(PerfilUsuario perfil)
    {
        if (!Enum.IsDefined(perfil))
            throw new ValidationException("Perfil do usuário é inválido, valor não definido");
        
        Perfil = perfil;
    }

    public void SetAtivo(bool ativo) =>
        Ativo = ativo;
    
    public override string ToString() =>
        $"{Nome} - {Email} - {Id}";

}