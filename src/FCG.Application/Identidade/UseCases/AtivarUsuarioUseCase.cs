using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Application.Identidade.UseCases;

public sealed class AtivarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    
    public AtivarUsuarioUseCase
    (
        IUsuarioRepository usuarioRepository
    )
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task Executar(Guid usuarioId)
    {
        Usuario usuario = await _usuarioRepository.ObterPorIdTracking(usuarioId)
            ?? throw new ValidationException("Usuário não existe");
        
        usuario.SetAtivo(true);

        await _usuarioRepository.UnitOfWork.Commit();
    }
}