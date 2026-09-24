using FCG.Domain.Identidade.ValueObjects;

namespace FCG.Domain.Identidade.Security;

public interface ISenhaHasher
{
    SenhaHash GerarHash(SenhaTextoPuro senhaTextoPuro);
    bool ValidarSenha(string senhaTextoPuro, SenhaHash senhaHash);
}