using ConexaoSolidaria.Domain.Identidade.Entities;

namespace ConexaoSolidaria.Domain.Identidade.Services
{
	public interface IRefreshTokenDomainService
	{
		Task RevogarCadeiaDescendente(RefreshToken refreshToken, Guid refreshTokenId);
	}
}
