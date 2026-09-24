using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using FCG.BDDTests.Support;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.ValueObjects;
using Moq;
using RefreshTokenEntity = FCG.Domain.Identidade.Entities.RefreshToken;

namespace FCG.BDDTests.Identidade.RefreshToken
{
	[Binding]
	public sealed class RefreshTokenSteps
	{
		private readonly RefreshTokenContext _context;

		public RefreshTokenSteps(RefreshTokenContext context)
		{
			_context = context;

			_context.RefreshTokenRepository
				.SetupGet(x => x.UnitOfWork)
				.Returns(new FakeUnitOfWork());

			_context.TokenSettings
				.SetupGet(x => x.ExpiracaoAccessTokenMinutos)
				.Returns(15);

			_context.TokenSettings
				.SetupGet(x => x.ExpiracaoRefreshTokenDias)
				.Returns(7);

			_context.TokenSettings
				.SetupGet(x => x.HabilitarSegurancaDeReusoRefreshToken)
				.Returns(false);

			_context.JwtService
				.Setup(x => x.GerarAccessToken(It.IsAny<Usuario>()))
				.Returns("novo-access-token");

			_context.RefreshTokenRepository
				.Setup(x => x.Adicionar(It.IsAny<RefreshTokenEntity>()))
				.Returns(Task.CompletedTask);
		}

		[Given(@"que existe um refresh token válido para renovação")]
		public void DadoExisteRefreshTokenValido()
		{
			var refreshToken = new RefreshTokenEntity(Guid.NewGuid(), 7);
			_context.RefreshTokenId = refreshToken.Id;

			_context.RefreshTokenRepository
				.Setup(x => x.ObterPorToken(_context.RefreshTokenId))
				.ReturnsAsync(refreshToken);
		}

		[Given(@"que existe um refresh token revogado")]
		public void DadoExisteRefreshTokenRevogado()
		{
			var refreshToken = new RefreshTokenEntity(Guid.NewGuid(), 7);
			refreshToken.Revogar(MotivoRevogacaoRefreshToken.Logout);
			_context.RefreshTokenId = refreshToken.Id;

			_context.RefreshTokenRepository
				.Setup(x => x.ObterPorToken(_context.RefreshTokenId))
				.ReturnsAsync(refreshToken);
		}

		[Given(@"que refresh token é inexistente")]
		public void DadoRefreshTokenInexistente()
		{
			_context.RefreshTokenId = Guid.NewGuid();

			_context.RefreshTokenRepository
				.Setup(x => x.ObterPorToken(_context.RefreshTokenId))
				.ReturnsAsync((RefreshTokenEntity?)null);
		}

		[Given(@"o usuário associado ao refresh token existe")]
		public void DadoUsuarioAssociadoExiste()
		{
			_context.UsuarioRepository
				.Setup(x => x.ObterPorId(It.IsAny<Guid>()))
				.ReturnsAsync(new Usuario(
					"Gabriel",
					new Email("gabriel@email.com"),
					new SenhaHash(new string('A', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
					PerfilUsuario.Usuario
				));
		}

		[When(@"a renovação do token é realizada")]
		public async Task QuandoRenovacaoEhRealizada()
		{
			var useCase = new RefreshTokenUseCase(
				_context.Logger.Object,
				_context.UsuarioRepository.Object,
				_context.RefreshTokenRepository.Object,
				_context.JwtService.Object,
				_context.TokenSettings.Object,
				_context.RefreshTokenDomainService.Object
			);

			try
			{
				_context.Response = await useCase.Executar(
					new RefreshRequest(_context.RefreshTokenId)
				);
			}
			catch (Exception ex)
			{
				_context.Excecao = ex;
			}
		}

		[Then(@"deve ser retornado um novo access token")]
		public void EntaoDeveRetornarNovoAccessToken()
		{
			_context.Response!.AccessToken.Should().Be("novo-access-token");
		}

		[Then(@"deve ser gerado um novo refresh token")]
		public void EntaoDeveGerarNovoRefreshToken()
		{
			_context.RefreshTokenRepository.Verify(
				x => x.Adicionar(It.IsAny<RefreshTokenEntity>()),
				Times.Once
			);
		}

		[Then(@"deve ocorrer um erro de autenticação ao renovar o token")]
		public void EntaoErroAutenticacaoAoRenovarToken()
		{
			_context.Excecao.Should().BeOfType<UnauthorizedAccessException>();
		}
	}
}