using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using FCG.Infrastructure.Identidade.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = RoleNames.Admin)]
public class UsuarioController : ControllerBase
{
    private readonly  AtivarUsuarioUseCase _ativarUsuarioUseCase;
    private readonly InativarUsuarioUseCase _inativarUsuarioUseCase;
    private readonly ListarTodosUsuariosUseCase _listarTodosUsuariosUseCase;
    private readonly ObterUsuarioPorIdUseCase _obterUsuarioPorIdUseCase;
    private readonly TornarUsuarioAdministradorUseCase _tornarUsuarioAdministradorUseCase;
    
    public UsuarioController
    (
        AtivarUsuarioUseCase ativarUsuarioUseCase,
        InativarUsuarioUseCase inativarUsuarioUseCase,
        ListarTodosUsuariosUseCase listarTodosUsuariosUseCase,
        ObterUsuarioPorIdUseCase obterUsuarioPorIdUseCase,
        TornarUsuarioAdministradorUseCase tornarUsuarioAdministradorUseCase
    )
    {
        _ativarUsuarioUseCase = ativarUsuarioUseCase;
        _inativarUsuarioUseCase = inativarUsuarioUseCase;
        _listarTodosUsuariosUseCase = listarTodosUsuariosUseCase;
        _obterUsuarioPorIdUseCase = obterUsuarioPorIdUseCase;
        _tornarUsuarioAdministradorUseCase = tornarUsuarioAdministradorUseCase;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Listar()
    {
        IEnumerable<UsuarioDto> usuarios = await _listarTodosUsuariosUseCase.Executar();
        
        return Ok(usuarios);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDto>> Obter(Guid id)
    {
        UsuarioDto? usuario = await _obterUsuarioPorIdUseCase.Executar(id);
        
        return Ok(usuario);
    }

    [HttpPatch("{id:guid}/tornar-administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> TornarUsuarioAdministrador(Guid id)
    {
        await _tornarUsuarioAdministradorUseCase.Executar(id);
        
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> InativarUsuario(Guid id)
    {
        await _inativarUsuarioUseCase.Executar(id);
        
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> AtivarUsuario(Guid id)
    {
        await _ativarUsuarioUseCase.Executar(id);
        
        return NoContent();
    }
    
}