using FCG.Domain.Identidade.Entities;

namespace FCG.Application.Identidade.Security;

public interface IJwtService
{
    string GerarAccessToken(Usuario usuario);
}