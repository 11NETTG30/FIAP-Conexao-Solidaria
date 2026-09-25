using ConexaoSolidaria.Application.Campanhas.DTOs;
using ConexaoSolidaria.Application.Campanhas.UseCases;
using ConexaoSolidaria.Infrastructure.Identidade.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("api/campanhas")]
public sealed class CampanhaController : ControllerBase
{
    private readonly CriarCampanhaUseCase _criarCampanhaUseCase;
    private readonly EditarCampanhaUseCase _editarCampanhaUseCase;
    private readonly ListarCampanhasAtivasUseCase _listarCampanhasAtivasUseCase;

    public CampanhaController
    (
        CriarCampanhaUseCase criarCampanhaUseCase,
        EditarCampanhaUseCase editarCampanhaUseCase,
        ListarCampanhasAtivasUseCase listarCampanhasAtivasUseCase
    )
    {
        _criarCampanhaUseCase = criarCampanhaUseCase;
        _editarCampanhaUseCase = editarCampanhaUseCase;
        _listarCampanhasAtivasUseCase = listarCampanhasAtivasUseCase;
    }

    // Painel de transparência — público
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CampanhaAtivaDto>>> ListarAtivas()
    {
        IEnumerable<CampanhaAtivaDto> campanhas = await _listarCampanhasAtivasUseCase.Executar();

        return Ok(campanhas);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.GestorONG)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CriarCampanhaResponse>> Criar([FromBody] CriarCampanhaRequest request)
    {
        Guid id = await _criarCampanhaUseCase.Executar(request);

        return StatusCode(StatusCodes.Status201Created, new CriarCampanhaResponse(id));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.GestorONG)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Editar(Guid id, [FromBody] EditarCampanhaRequest request)
    {
        await _editarCampanhaUseCase.Executar(id, request);

        return NoContent();
    }
}
