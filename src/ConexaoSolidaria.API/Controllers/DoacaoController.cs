using ConexaoSolidaria.Application.Doacoes.DTOs;
using ConexaoSolidaria.Application.Doacoes.UseCases;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Infrastructure.Identidade.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("api/doacoes")]
public sealed class DoacaoController : ControllerBase
{
    private readonly RegistrarIntencaoDoacaoUseCase _registrarIntencaoDoacaoUseCase;
    private readonly ObterDoacaoPorIdUseCase _obterDoacaoPorIdUseCase;
    private readonly ListarDoacoesUseCase _listarDoacoesUseCase;
    private readonly ListarDoacoesAdminUseCase _listarDoacoesAdminUseCase;

    public DoacaoController
    (
        RegistrarIntencaoDoacaoUseCase registrarIntencaoDoacaoUseCase,
        ObterDoacaoPorIdUseCase obterDoacaoPorIdUseCase,
        ListarDoacoesUseCase listarDoacoesUseCase,
        ListarDoacoesAdminUseCase listarDoacoesAdminUseCase
    )
    {
        _registrarIntencaoDoacaoUseCase = registrarIntencaoDoacaoUseCase;
        _obterDoacaoPorIdUseCase = obterDoacaoPorIdUseCase;
        _listarDoacoesUseCase = listarDoacoesUseCase;
        _listarDoacoesAdminUseCase = listarDoacoesAdminUseCase;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Doador)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CriarDoacaoResponse>> Criar([FromBody] CriarDoacaoRequest request)
    {
        Guid id = await _registrarIntencaoDoacaoUseCase.Executar(request);

        return StatusCode(StatusCodes.Status202Accepted, new CriarDoacaoResponse(id));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleNames.Doador)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoacaoDto>> ObterPorId(Guid id)
    {
        DoacaoDto doacao = await _obterDoacaoPorIdUseCase.Executar(id);

        return Ok(doacao);
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Doador)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaDto<DoacaoDto>>> Listar([FromQuery] FiltroListagemDoacaoRequest request)
    {
        PaginaDto<DoacaoDto> pagina = await _listarDoacoesUseCase.Executar(request);

        return Ok(pagina);
    }

    [HttpGet("admin")]
    [Authorize(Roles = RoleNames.GestorONG)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaDto<DoacaoDto>>> ListarAdmin([FromQuery] FiltroListagemDoacaoAdminRequest request)
    {
        PaginaDto<DoacaoDto> pagina = await _listarDoacoesAdminUseCase.Executar(request);

        return Ok(pagina);
    }
}
