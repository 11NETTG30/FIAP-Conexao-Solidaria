using FCG.Domain.Identidade.Entities;

namespace FCG.Application.Identidade.DTOs;

public record UsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
)
{
    public static explicit operator UsuarioDto(Usuario usuario)
    {
        return new UsuarioDto(
            Id: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email.Valor,
            Perfil: usuario.Perfil.ToString(),
            Ativo: usuario.Ativo,
            DataCriacao: usuario.DataCriacao,
            DataAtualizacao: usuario.DataAtualizacao
        );
    }
}