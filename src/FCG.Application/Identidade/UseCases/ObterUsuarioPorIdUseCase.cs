using FCG.Application.Identidade.DTOs;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;

namespace FCG.Application.Identidade.UseCases;

public sealed class ObterUsuarioPorIdUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterUsuarioPorIdUseCase
    (
        IUsuarioRepository usuarioRepository
    )
    {
        _usuarioRepository = usuarioRepository;
    }
    
    public async Task<UsuarioDto?> Executar(Guid id)
    {
        Usuario? usuario = await _usuarioRepository.ObterPorId(id);

        if (usuario is null)
            return null;
        
        return (UsuarioDto)usuario;
    }
}