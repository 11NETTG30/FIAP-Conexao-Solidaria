using FCG.Application.Identidade.DTOs;
using FCG.Application.Identidade.UseCases;
using FCG.BDDTests.Identidade.Login;
using FCG.BDDTests.Support;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.ValueObjects;
using Moq;

[Binding]
public sealed class LoginSteps
{
	private readonly LoginContext _context;

	public LoginSteps(LoginContext context)
	{
		_context = context;

		_context.RefreshTokenRepository
			.SetupGet(x => x.UnitOfWork)
			.Returns(new FakeUnitOfWork());

		_context.TokenSettings
			.SetupGet(x => x.ExpiracaoAccessTokenMinutos)
			.Returns(15);

		_context.TokenSettings
			.SetupGet(x => x.ExpiracaoRefreshTokenDias)
			.Returns(7);

		_context.JwtService
			.Setup(x => x.GerarAccessToken(It.IsAny<Usuario>()))
			.Returns("access-token-valido");

		_context.RefreshTokenRepository
			.Setup(x => x.Adicionar(It.IsAny<RefreshToken>()))
			.Returns(Task.CompletedTask);
	}

	[Given(@"que existe um usuário cadastrado para login com e-mail ""(.*)"" e senha válida")]
	public void DadoQueExisteUsuarioComSenhaValida(string email)
	{
		var usuario = CriarUsuarioFake(email);

		_context.UsuarioRepository
			.Setup(x => x.ObterPorEmail(email))
			.ReturnsAsync(usuario);

		_context.SenhaHasher
			.Setup(x => x.ValidarSenha(It.IsAny<string>(), usuario.SenhaHash))
			.Returns(true);
	}

	[Given(@"que existe um usuário cadastrado para login com e-mail ""(.*)"" e senha inválida")]
	public void DadoQueExisteUsuarioComSenhaInvalida(string email)
	{
		var usuario = CriarUsuarioFake(email);

		_context.UsuarioRepository
			.Setup(x => x.ObterPorEmail(email))
			.ReturnsAsync(usuario);

		_context.SenhaHasher
			.Setup(x => x.ValidarSenha(It.IsAny<string>(), usuario.SenhaHash))
			.Returns(false);
	}

	[Given(@"que não existe usuário cadastrado para login com o e-mail ""(.*)""")]
	public void DadoQueNaoExisteUsuario(string email)
	{
		_context.UsuarioRepository
			.Setup(x => x.ObterPorEmail(email))
			.ReturnsAsync((Usuario?)null);
	}

	[When(@"o login é realizado com e-mail ""(.*)"" e senha ""(.*)""")]
	public async Task QuandoLoginEhRealizado(string email, string senha)
	{
		var useCase = new LoginUseCase(
			_context.UsuarioRepository.Object,
			_context.RefreshTokenRepository.Object,
			_context.JwtService.Object,
			_context.SenhaHasher.Object,
			_context.TokenSettings.Object
		);

		var request = new LoginRequest(email, senha);

		try
		{
			_context.Response = await useCase.Executar(request);
		}
		catch (Exception ex)
		{
			_context.Excecao = ex;
		}
	}

	[Then(@"deve ser retornado um access token")]
	public void EntaoDeveRetornarAccessToken()
	{
		_context.Excecao.Should().BeNull();
		_context.Response!.AccessToken.Should().Be("access-token-valido");
	}

	[Then(@"deve ser gerado um refresh token")]
	public void EntaoDeveGerarRefreshToken()
	{
		_context.Excecao.Should().BeNull();
		_context.RefreshTokenRepository.Verify(
			x => x.Adicionar(It.IsAny<RefreshToken>()),
			Times.Once
		);
	}

	[Then(@"deve ocorrer um erro de autenticação")]
	public void EntaoDeveOcorrerErroAutenticacao()
	{
		_context.Excecao.Should().BeOfType<UnauthorizedAccessException>();
	}

	private static Usuario CriarUsuarioFake(string email)
	{
		return new Usuario(
			"Gabriel",
			new Email(email),
			new SenhaHash(new string('A', SenhaHash.TAMANHO_ESPERADO_SENHA_HASH)),
			PerfilUsuario.Usuario
		);
	}
}
