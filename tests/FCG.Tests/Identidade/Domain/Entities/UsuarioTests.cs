using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Tests.Identidade.Domain.Entities
{
    public class UsuarioTests
    {
        private const string NomeValido = "João Silva";
        private const string EmailValido = "joao.silva@fcg.com.br";
        private const string SenhaHashValida = "qicmeEIFx8xP5qg0bDWjSw==.3FN7adyhH5S4RwAjj1RfM3Kzt4m2YNeuscD6QTxxSN8=";

        [Fact]
        public void Construtor_DevecriarUsuario_QuandoDadosValidos()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var usuario = new Usuario(NomeValido, email, senhaHash, perfil);

            // Assert
            Assert.Equal(NomeValido, usuario.Nome);
            Assert.Equal(email, usuario.Email);
            Assert.Equal(senhaHash, usuario.SenhaHash);
            Assert.Equal(perfil, usuario.Perfil);
            Assert.True(usuario.Ativo);
            Assert.NotEqual(Guid.Empty, usuario.Id);
            Assert.Empty(usuario.RefreshTokens);
        }

        #region SetNome Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SetNome_DeveLancarException_QuandoNomeVazioOuNulo(string nomeInvalido)
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var acao = () => new Usuario(nomeInvalido, email, senhaHash, perfil);

            // Assert
            ValidationException ex = Assert.Throws<ValidationException>(acao);
            Assert.Equal("Nome não pode ser vazio ou nulo", ex.Message);
        }

        [Theory]
        [InlineData("A")] // 1 caractere
        [InlineData("AB")] // 2 caracteres - válido
        public void SetNome_DeveLancarException_QuandoNomeMenorQue2Caracteres(string nome)
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            if (nome.Length < 2)
            {
                // Act
                var acao = () => new Usuario(nome, email, senhaHash, perfil);

                // Assert
                ValidationException ex = Assert.Throws<ValidationException>(acao);
                Assert.Equal("Nome deve ter entre 2 e 100 caracteres", ex.Message);
            }
            else
            {
                // Act & Assert - não deve lançar exceção
                var usuario = new Usuario(nome, email, senhaHash, perfil);
                Assert.Equal(nome, usuario.Nome);
            }
        }

        [Fact]
        public void SetNome_DeveLancarException_QuandoNomeMaiorQue100Caracteres()
        {
            // Arrange
            var nomeGrande = new string('A', 101);
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var acao = () => new Usuario(nomeGrande, email, senhaHash, perfil);

            // Assert
            ValidationException ex = Assert.Throws<ValidationException>(acao);
            Assert.Equal("Nome deve ter entre 2 e 100 caracteres", ex.Message);
        }

        [Theory]
        [InlineData("  João Silva  ", "João Silva")]
        [InlineData("Maria  Santos", "Maria  Santos")]
        public void SetNome_DeveRemoverEspacosExternos(string nomeComEspacos, string nomeEsperado)
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var usuario = new Usuario(nomeComEspacos, email, senhaHash, perfil);

            // Assert
            Assert.Equal(nomeEsperado, usuario.Nome);
        }

        [Fact]
        public void SetNome_DeveAtualizarNome_QuandoChamadoAposInstanciacao()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);
            var novoNome = "Maria Santos";

            // Act
            usuario.SetNome(novoNome);

            // Assert
            Assert.Equal(novoNome, usuario.Nome);
        }

        #endregion

        #region SetEmail Tests

        [Fact]
        public void SetEmail_DeveLancarException_QuandoEmailNulo()
        {
            // Arrange
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var acao = () => new Usuario(NomeValido, null!, senhaHash, perfil);

            // Assert
            ValidationException ex = Assert.Throws<ValidationException>(acao);
            Assert.Equal("E-mail é obrigatório", ex.Message);
        }

        [Fact]
        public void SetEmail_DeveAtualizarEmail_QuandoChamadoAposInstanciacao()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);
            var novoEmail = new Email("novo.email@fcg.com.br");

            // Act
            usuario.SetEmail(novoEmail);

            // Assert
            Assert.Equal(novoEmail, usuario.Email);
        }

        #endregion

        #region SetSenhaHash Tests

        [Fact]
        public void SetSenhaHash_DeveLancarException_QuandoSenhaHashNula()
        {
            // Arrange
            var email = new Email(EmailValido);
            var perfil = PerfilUsuario.Usuario;

            // Act
            var acao = () => new Usuario(NomeValido, email, null!, perfil);

            // Assert
            ValidationException ex = Assert.Throws<ValidationException>(acao);
            Assert.Equal("Senha é obrigatória", ex.Message);
        }

        [Fact]
        public void SetSenhaHash_DeveAtualizarSenhaHash_QuandoChamadoAposInstanciacao()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);
            var novaSenhaHash = new SenhaHash("NovaSenhaHash123==./abc1231231231231231231231231231231231231231234567");

            // Act
            usuario.SetSenhaHash(novaSenhaHash);

            // Assert
            Assert.Equal(novaSenhaHash, usuario.SenhaHash);
        }

        #endregion

        #region SetPerfil Tests

        [Fact]
        public void SetPerfil_DeveLancarException_QuandoPerfilInvalido()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var perfilInvalido = 999;

            // Act
            var acao = () => new Usuario(NomeValido, email, senhaHash, (PerfilUsuario)perfilInvalido);

            // Assert
            ValidationException ex = Assert.Throws<ValidationException>(acao);
            Assert.Equal("Perfil do usuário é inválido, valor não definido", ex.Message);
        }

        [Theory]
        [InlineData(PerfilUsuario.Usuario)]
        [InlineData(PerfilUsuario.Administrador)]
        public void SetPerfil_DeveDefinirPerfil_QuandoPerfilValido(PerfilUsuario perfil)
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);

            // Act
            var usuario = new Usuario(NomeValido, email, senhaHash, perfil);

            // Assert
            Assert.Equal(perfil, usuario.Perfil);
        }

        [Fact]
        public void SetPerfil_DeveAtualizarPerfil_QuandoChamadoAposInstanciacao()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);

            // Act
            usuario.SetPerfil(PerfilUsuario.Administrador);

            // Assert
            Assert.Equal(PerfilUsuario.Administrador, usuario.Perfil);
        }

        #endregion

        #region SetAtivo Tests

        [Fact]
        public void SetAtivo_DeveDefinirComoAtivo_QuandoVerdadeiro()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);

            // Act
            usuario.SetAtivo(true);

            // Assert
            Assert.True(usuario.Ativo);
        }

        [Fact]
        public void SetAtivo_DeveDefinirComoInativo_QuandoFalso()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);

            // Act
            usuario.SetAtivo(false);

            // Assert
            Assert.False(usuario.Ativo);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_DeveRetornarFormatoCorreto()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);

            // Act
            var resultado = usuario.ToString();

            // Assert
            Assert.Contains(NomeValido, resultado);
            Assert.Contains(EmailValido, resultado);
            Assert.Contains(usuario.Id.ToString(), resultado);
            Assert.Matches(@".+ - .+ - .+", resultado);
        }

        #endregion

        #region RefreshTokens Tests

        [Fact]
        public void RefreshTokens_DeveRetornarColecaoVazia_QuandoInstanciado()
        {
            // Arrange
            var email = new Email(EmailValido);
            var senhaHash = new SenhaHash(SenhaHashValida);

            // Act
            var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Usuario);

            // Assert
            Assert.Empty(usuario.RefreshTokens);
            Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<RefreshToken>>(usuario.RefreshTokens);
        }

        #endregion
    }
}
