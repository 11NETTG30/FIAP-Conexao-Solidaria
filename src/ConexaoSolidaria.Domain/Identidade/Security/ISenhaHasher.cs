using ConexaoSolidaria.Domain.Identidade.ValueObjects;

namespace ConexaoSolidaria.Domain.Identidade.Security;

public interface ISenhaHasher
{
    SenhaHash GerarHash(SenhaTextoPuro senhaTextoPuro);
    bool ValidarSenha(string senhaTextoPuro, SenhaHash senhaHash);
}