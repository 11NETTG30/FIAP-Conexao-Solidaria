using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/conta")]
[Authorize]
public sealed class ContaController : ControllerBase
{
    private readonly AlterarSenhaUseCase _alterarSenhaUseCase;
    private readonly ObterContaUseCase _obterContaUseCase;
    
    public ContaController
    (
        AlterarSenhaUseCase alterarSenhaUseCase,
        ObterContaUseCase obterContaUseCase
    )
    {
        _alterarSenhaUseCase = alterarSenhaUseCase;
        _obterContaUseCase = obterContaUseCase;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> Obter()
    {
        UsuarioDto? usuario = await _obterContaUseCase.Executar();
        
        return Ok(usuario);
    }
    
    [HttpPatch("senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
    {
        await _alterarSenhaUseCase.Executar(request);
        
        return NoContent();
    }
    
}