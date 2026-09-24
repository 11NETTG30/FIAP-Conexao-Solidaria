using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.Security;
using FCG.Application.Identidade.UseCases;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Services;
using Moq;

namespace FCG.BDDTests.Identidade.RefreshToken
{
	public sealed class RefreshTokenContext
	{
		public Mock<IUsuarioRepository> UsuarioRepository { get; } = new();
		public Mock<IRefreshTokenRepository> RefreshTokenRepository { get; } = new();
		public Mock<IJwtService> JwtService { get; } = new();
		public Mock<ITokenSettings> TokenSettings { get; } = new();
		public Mock<IRefreshTokenDomainService> RefreshTokenDomainService { get; } = new();
		public Mock<Microsoft.Extensions.Logging.ILogger<RefreshTokenUseCase>> Logger { get; } = new();
		public AuthResponse? Response { get; set; }
		public Exception? Excecao { get; set; }
		public Guid RefreshTokenId { get; set; }
	}
}