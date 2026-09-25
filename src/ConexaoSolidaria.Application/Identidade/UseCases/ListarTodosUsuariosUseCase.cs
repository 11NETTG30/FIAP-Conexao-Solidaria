using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Repositories;

namespace ConexaoSolidaria.Application.Identidade.UseCases;

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