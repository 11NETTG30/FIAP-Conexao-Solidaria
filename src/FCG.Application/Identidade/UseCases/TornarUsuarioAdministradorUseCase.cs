using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Application.Identidade.UseCases;

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

        usuario.SetPerfil(PerfilUsuario.Administrador);
        
        await _usuarioRepository.UnitOfWork.Commit();
    }
}