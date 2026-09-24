using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using FCG.BDDTests.Support;
using FCG.Domain.Identidade.Entities;
using Moq;
using RefreshTokenEntity = FCG.Domain.Identidade.Entities.RefreshToken;

namespace FCG.BDDTests.Identidade.Logout
{
	[Binding]
	public sealed class LogoutSteps
	{
		private readonly LogoutContext _context;
		private Guid _refreshToken;

		public LogoutSteps(LogoutContext context)
		{
			_context = context;

			_context.RefreshTokenRepository
				.SetupGet(x => x.UnitOfWork)
				.Returns(new FakeUnitOfWork());
		}

		[Given(@"que existe um refresh token válido")]
		public void DadoQueExisteRefreshTokenValido()
		{
			_refreshToken = Guid.NewGuid();

			_context.RefreshTokenRepository
				.Setup(x => x.ObterPorToken(_refreshToken))
				.ReturnsAsync(new RefreshTokenEntity(Guid.NewGuid(), 7));
		}

		[Given(@"que não existe refresh token")]
		public void DadoQueNaoExisteRefreshToken()
		{
			_refreshToken = Guid.NewGuid();

			_context.RefreshTokenRepository
				.Setup(x => x.ObterPorToken(_refreshToken))
				.ReturnsAsync((RefreshTokenEntity?)null);
		}

		[When(@"o logout é realizado")]
		public async Task QuandoLogoutEhRealizado()
		{
			var useCase = new LogoutUseCase(
				_context.RefreshTokenRepository.Object
			);

			await useCase.Executar(
				new LogoutRequest(_refreshToken)
			);
		}

		[Then(@"o refresh token deve ser revogado")]
		public void EntaoRefreshTokenDeveSerRevogado()
		{
			_context.RefreshTokenRepository.Verify(
				x => x.ObterPorToken(_refreshToken),
				Times.Once
			);
		}

		[Then(@"nenhuma revogação deve ocorrer")]
		public void EntaoNenhumaRevogacaoDeveOcorrer()
		{
			_context.RefreshTokenRepository.Verify(
				x => x.ObterPorToken(_refreshToken),
				Times.Once
			);
		}
	}
}
