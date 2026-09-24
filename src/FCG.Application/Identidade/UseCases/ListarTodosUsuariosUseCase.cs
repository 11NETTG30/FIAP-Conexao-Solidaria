using FCG.Application.Identidade.DTOs;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;

namespace FCG.Application.Identidade.UseCases;

public sealed class ListarTodosUsuariosUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarTodosUsuariosUseCase
    (
        IUsuarioRepository usuarioRepository
    )
    {
        _usuarioRepository = usuarioRepository;
    }
    
    public async Task<IEnumerable<UsuarioDto>> Executar()
    {
        List<Usuario> usuarios = await _usuarioRepository.ListarTodos();

        return usuarios.Select(usuario => (UsuarioDto)usuario);
    }
}