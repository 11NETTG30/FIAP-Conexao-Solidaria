using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Application.Identidade.UseCases;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Shared.UoW;
using Moq;

namespace ConexaoSolidaria.Tests.Identidade.Application.UseCases;

public class LogoutUseCaseTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LogoutUseCase _useCase;

    public LogoutUseCaseTests()
    {
        _refreshTokenRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _useCase = new LogoutUseCase(_refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task AoLogoutComTokenInexistenteNaoDeveLancarExcecaoNemCommitar()
    {
        // Arrange
        Guid tokenInexistente = Guid.NewGuid();
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(tokenInexistente))
            .ReturnsAsync((RefreshToken?)null);

        LogoutRequest request = new(tokenInexistente);

        // Act
        Exception? ex = await Record.ExceptionAsync(() => _useCase.Executar(request));

        // Assert
        Assert.Null(ex);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task AoLogoutComTokenValidoDeveRevogarTokenEChamarCommit()
    {
        // Arrange
        RefreshToken token = new(Guid.NewGuid(), 7);
        _refreshTokenRepositoryMock
            .Setup(r => r.ObterPorToken(token.Token))
            .ReturnsAsync(token);

        LogoutRequest request = new(token.Token);

        // Act
        await _useCase.Executar(request);

        // Assert
        Assert.True(token.Revogado);
        Assert.Equal(MotivoRevogacaoRefreshToken.Logout, token.MotivoRevogacao);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}
