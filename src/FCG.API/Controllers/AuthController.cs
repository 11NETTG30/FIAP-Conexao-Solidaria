using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly LogoutUseCase _logoutUseCase;
    private readonly CriarUsuarioUseCase _criarUsuarioUseCase;

    public AuthController
    (
        LoginUseCase loginUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        LogoutUseCase logoutUseCase,
        CriarUsuarioUseCase criarUsuarioUseCase
    )
    {
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
        _criarUsuarioUseCase = criarUsuarioUseCase;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> Registrar([FromBody] CriarUsuarioRequest request)
    {
        await _criarUsuarioUseCase.Executar(request);

        return Created();
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        AuthResponse response = await _loginUseCase.Executar(request);
        
        return Ok(response);
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Refresh([FromBody] RefreshRequest request)
    {
        AuthResponse response = await _refreshTokenUseCase.Executar(request);
        
        return Ok(response);
    }
    
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _logoutUseCase.Executar(request);
        
        return NoContent();
    }
    
}