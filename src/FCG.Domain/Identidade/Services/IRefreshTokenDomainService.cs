using FCG.Domain.Identidade.Entities;

namespace FCG.Domain.Identidade.Services
{
	public interface IRefreshTokenDomainService
	{
		Task RevogarCadeiaDescendente(RefreshToken refreshToken, Guid refreshTokenId);
	}
}
