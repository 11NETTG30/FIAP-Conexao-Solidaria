using ConexaoSolidaria.Domain.Identidade.Entities;

namespace ConexaoSolidaria.Application.Identidade.Security;

public interface IJwtService
{
    string GerarAccessToken(Usuario usuario);
}