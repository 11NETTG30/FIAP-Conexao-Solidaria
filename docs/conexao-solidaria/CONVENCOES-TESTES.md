# Convenções de testes unitários

Referência rápida para escrever testes consistentes com o que já existe em
`tests/ConexaoSolidaria.Tests`, sem precisar garimpar o código antes.
Só testes **unitários** — sem integração, sem BDD (os testes Reqnroll foram
removidos deliberadamente e não devem voltar).

## Nome dos métodos de teste

Sempre começa com `Ao`, nunca usa underline. Formato geral:

```
Ao<Ação/Cenário>Deve<ResultadoEsperado>
```

Exemplos reais do repo (`CampanhaTests.cs`):

```csharp
AoCriarCampanhaComDataFimNoPassadoDeveLancarExcecao
AoCriarCampanhaComMetaFinanceiraMenorOuIgualAZeroDeveLancarExcecao
AoVerificarEdicaoDeCampanhaEncerradaDeveLancarExcecao
```

> Os testes mais antigos de `UsuarioTests.cs` usam um padrão anterior
> (`SetNome_DeveLancarException_QuandoNomeVazioOuNulo`, com underline). Não
> reescrever os que já existem e passam — mas todo teste **novo** segue o
> padrão `Ao...` acima, sem underline.

## Estrutura AAA (Arrange/Act/Assert)

Os três blocos são explícitos, geralmente com comentário marcando cada um
(exceto em testes bem curtos de uma linha, onde o comentário é dispensável).
Exemplo real (`CampanhaTests.cs`):

```csharp
[Fact]
public void AoCriarCampanhaComDataFimNoPassadoDeveLancarExcecao()
{
    DateTime dataInicio = DateTime.UtcNow.AddDays(-10);
    DateTime dataFim = DateTime.UtcNow.AddDays(-1);

    ValidationException ex = Assert.Throws<ValidationException>(() =>
        new Campanha(TituloValido, DescricaoValida, dataInicio, dataFim, MetaValida));

    Assert.Equal("Data de término da campanha não pode estar no passado", ex.Message);
}
```

Exemplo com Arrange/Act/Assert nomeados explicitamente (`UsuarioTests.cs`):

```csharp
[Fact]
public void SetAtivo_DeveDefinirComoAtivo_QuandoVerdadeiro()
{
    // Arrange
    var email = new Email(EmailValido);
    var senhaHash = new SenhaHash(SenhaHashValida);
    var usuario = new Usuario(NomeValido, email, senhaHash, PerfilUsuario.Doador);

    // Act
    usuario.SetAtivo(true);

    // Assert
    Assert.True(usuario.Ativo);
}
```

Cenários de erro/borda preferem `[Theory]` + `[InlineData]` (ou
`[MemberData]` quando o valor não pode ser um literal de `InlineData`, como
`decimal`) em vez de repetir o mesmo teste várias vezes.

## Mocks

Biblioteca: **Moq** (`tests/ConexaoSolidaria.Tests/ConexaoSolidaria.Tests.csproj`).
Até a expansão de cobertura dos casos de uso não havia nenhuma lib de mock
referenciada, porque só existiam testes de Domain (entidades e value
objects), que não têm dependências para mockar. Ao testar um `UseCase`,
mocka-se as interfaces injetadas no construtor (`IUsuarioRepository`,
`ISenhaHasher`, etc.) — nunca a implementação concreta.

Padrão usado (`Mock<T>` + `Setup` + `Returns`/`ThrowsAsync`, verificação com
`Verify` quando o efeito colateral importa):

```csharp
var usuarioRepositoryMock = new Mock<IUsuarioRepository>();
var unitOfWorkMock = new Mock<IUnitOfWork>();
usuarioRepositoryMock.Setup(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);
usuarioRepositoryMock
    .Setup(r => r.VerificarExistenciaEmail(It.IsAny<string>()))
    .ReturnsAsync(true);

var useCase = new CriarUsuarioUseCase(usuarioRepositoryMock.Object, senhaHasherMock.Object);

await Assert.ThrowsAsync<ConflictException>(() => useCase.Executar(request));

usuarioRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Usuario>()), Times.Never);
```

Repositórios (`IRepository<T>`) sempre expõem `UnitOfWork` — precisa mockar
`IUnitOfWork.Commit()` também quando o caminho testado chega a commitar.

## Onde ficam os testes

Espelha a estrutura de `src/`, por módulo e depois por camada:

```
tests/ConexaoSolidaria.Tests/
├── Identidade/
│   ├── Domain/
│   │   ├── Entities/        (Usuario, RefreshToken)
│   │   └── ValueObjects/    (Email, SenhaHash, SenhaTextoPuro)
│   └── Application/
│       └── UseCases/        (um arquivo por UseCase)
└── Campanhas/
    ├── Domain/
    │   └── Entities/        (Campanha)
    └── Application/
        └── UseCases/
```

Um arquivo de teste por classe testada, nome `<Classe>Tests.cs`, namespace
espelhando a pasta (ex.: `ConexaoSolidaria.Tests.Identidade.Application.UseCases`).

## Asserts

**xUnit puro** (`Assert.Equal`, `Assert.True`, `Assert.Throws`,
`Assert.ThrowsAsync`, `Record.Exception`, etc.) — não há FluentAssertions no
projeto, não introduzir sem necessidade.

## Escopo dos testes unitários

- Testar exceções de domínio (`ValidationException`, `ConflictException`,
  `NotFoundException`) e as regras que as disparam — não só o caminho feliz.
- Casos de uso: mockar todas as dependências injetadas; nunca tocar
  banco/HTTP real.
- Não criar testes de integração nem reintroduzir BDD/Reqnroll.
