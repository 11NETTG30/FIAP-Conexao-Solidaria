using System.Security.Cryptography;
using System.Text;
using FCG.Domain.Identidade.Security;
using FCG.Domain.Identidade.ValueObjects;
using Konscious.Security.Cryptography;

namespace FCG.Infrastructure.Identidade.Security
{
    public sealed class Argon2IdSenhaHasher : ISenhaHasher
    {
        private const int _MEMORIA_KB = 19 * 1024;
        private const int _ITERACOES = 2;
        private const int _GRAU_PARALELISMO = 1;
        private const int _TAMANHO_SALT_BYTES = 16;
        private const int _TAMANHO_HASH_BYTES = 32;
    
        public SenhaHash GerarHash(SenhaTextoPuro senhaTextoPuro)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_TAMANHO_SALT_BYTES);
            byte[] hash = CalcularHash(senhaTextoPuro.Senha, salt);

            string formatoArmazenamento = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    
            return new SenhaHash(formatoArmazenamento);
        }

        public bool ValidarSenha(string senhaTextoPuro, SenhaHash senhaHash)
        {
            SenhaHashInfo? senhaHashOriginal = ObterInformacoesSenhaHashOriginal(senhaHash.Senha);
    
            if (senhaHashOriginal is null)
                return false;
    
            byte[] hashTentativa = CalcularHash(senhaTextoPuro, senhaHashOriginal.Salt);

            // Comparação de tempo constante para evitar ataques de temporização
            return CryptographicOperations.FixedTimeEquals(senhaHashOriginal.Hash, hashTentativa);
        }

        private static byte[] CalcularHash(string senhaTextoPuro, byte[] salt)
        {
            using Argon2id argon2 = new(Encoding.UTF8.GetBytes(senhaTextoPuro))
            {
                Salt = salt,
                DegreeOfParallelism = _GRAU_PARALELISMO,
                Iterations = _ITERACOES,
                MemorySize = _MEMORIA_KB
            };

            return argon2.GetBytes(_TAMANHO_HASH_BYTES);
        }

        private static SenhaHashInfo? ObterInformacoesSenhaHashOriginal(string senhaHash)
        {
            if (string.IsNullOrEmpty(senhaHash) || !senhaHash.Contains('.'))
                return null;

            string[] partes = senhaHash.Split('.');
    
            if (partes.Length != 2)
                return null;
    
            try
            {
                byte[] salt = Convert.FromBase64String(partes[0]);
                byte[] hash = Convert.FromBase64String(partes[1]);

                if (salt.Length != _TAMANHO_SALT_BYTES || hash.Length != _TAMANHO_HASH_BYTES)
                    return null;

                return new SenhaHashInfo(salt, hash);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private record SenhaHashInfo(byte[] Salt, byte[] Hash);
    }
}