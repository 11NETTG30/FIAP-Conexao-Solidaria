using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Application.Identidade.UseCases;

public sealed class TornarUsuarioAdministradorUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public TornarUsuarioAdministradorUseCase
    (
        IUsuarioRepository usuarioRepository
    )
    {
        _usuarioRepository = usuarioRepository;
    }
    
    public async Task Executar(Guid id)
    {
        Usuario usuario = await _usuarioRepository.ObterPorIdTracking(id)
            ?? throw new ValidationException("Usuário não encontrado");

        usuario.SetPerfil(PerfilUsuario.GestorONG);
        
        await _usuarioRepository.UnitOfWork.Commit();
    }
}