using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.Security;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Security;
using Moq;

namespace FCG.BDDTests.Identidade.Login
{
	public sealed class LoginContext
	{
		public Mock<IUsuarioRepository> UsuarioRepository { get; } = new();
		public Mock<IRefreshTokenRepository> RefreshTokenRepository { get; } = new();
		public Mock<IJwtService> JwtService { get; } = new();
		public Mock<ISenhaHasher> SenhaHasher { get; } = new();
		public Mock<ITokenSettings> TokenSettings { get; } = new();
		public AuthResponse? Response { get; set; }
		public Exception? Excecao { get; set; }
	}
}
