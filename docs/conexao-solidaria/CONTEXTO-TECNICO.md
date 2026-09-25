# Contexto técnico do repositório

## Origem

Copiado e adaptado do repositório da Fase 1
(`github.com/11NETTG30/fiap-cloud-games`). **Não** partir do `fcg-users`
(microsserviço extraído para o desenho antigo com Kong + JWT RS256/JWKS —
incompatível com a arquitetura atual, que não tem gateway).

## Stack

| Categoria | Tecnologia |
| --- | --- |
| Plataforma | .NET 10 |
| Framework Web | ASP.NET |
| Linguagem | C# 14 |
| ORM | Entity Framework Core + Migrations |
| Banco | PostgreSQL (1 instância, 1 schema por módulo) |
| Documentação API | Swagger + Scalar |
| Autenticação | JWT HMAC simétrico + RefreshToken rotativo |
| Hash de senha | Argon2id (19 MiB, 2 iterações, paralelismo 1) — `Konscious.Security.Cryptography.Argon2` |
| Testes unitários | xUnit |
| Testes de mutação | Stryker.NET |
| Mensageria | RabbitMQ + MassTransit — **ainda não configurado**, entra na demanda d2 |

## Estrutura (Clean Architecture, 5 projetos na solução)

```
src/
├── ConexaoSolidaria.API/            ← Controllers, Program.cs, Middlewares, Configurations
├── ConexaoSolidaria.Application/     ← DTOs, UseCases, Validators (FluentValidation não usado ainda — ver nota)
├── ConexaoSolidaria.Domain/          ← Entidades, Value Objects, interfaces de repositório
├── ConexaoSolidaria.Infrastructure/  ← EF Core, JWT, Argon2id, DbContexts
└── ConexaoSolidaria.IoC/              ← composição de dependências (extension methods)

tests/
└── ConexaoSolidaria.Tests/
```

Cada camada tem subpastas por **módulo de negócio** — hoje só `Identidade`
existe; `Campanha` e `Doacao` entram como pastas novas, no mesmo nível:

```
src/ConexaoSolidaria.Domain/Identidade/{Entities, Enums, Repositories, Security, ValueObjects}
src/ConexaoSolidaria.Domain/Shared/Abstractions/{Entity, IAggregateRoot, IRepository, ValueObject, ...}
```

`Shared/` contém as abstrações cross-módulo (`Entity`, `IAggregateRoot`,
`IRepository<T>`, exceptions de domínio) — módulos novos reaproveitam essas
classes, não recriam.

## Padrão de módulo — Identidade como referência

Ao implementar `Campanha` ou `Doacao`, seguir exatamente esta forma (é o que
`Identidade` já faz):

1. **Domain**: entidade herda de `Entity` + `IAggregateRoot`; propriedades
   com setters privados, métodos `SetX()` para mutação controlada
2. **Application**: pasta `UseCases/` (um caso de uso por classe), `DTOs/`,
   `Validators/`
3. **Infrastructure**: `Persistence/<Modulo>DbContext.cs` com
   `public const string SCHEMA = "<nome>"`, aplicado via
   `modelBuilder.HasDefaultSchema(SCHEMA)`; `Persistence/Configurations/`
   para o `IEntityTypeConfiguration<T>` de cada entidade;
   `Persistence/Repositories/` implementando as interfaces do Domain
4. **Registro**: novo `DbContext` entra em
   `ConexaoSolidaria.Infrastructure/Configurations/DatabaseConfiguration.cs`, chamando o
   método genérico `AddDatabasePostgreSQL<T>(connectionString, schema)` que
   já existe ali; repositórios entram em
   `ConexaoSolidaria.IoC/DependencyInjectionInfrastructure.cs`, método `AddRepositories()`

## Autenticação — estado atual e o que muda

`JwtService.cs` já assina com **HMAC SHA256 simétrico**
(`SecurityAlgorithms.HmacSha256Signature`), chave em
`JwtSettings.Secret` (appsettings) — não precisa trocar nada aqui, só as
claims:

- Enum `PerfilUsuario` (`Usuario = 1, Administrador = 2`) → renomear para
  `Doador = 1, GestorONG = 2` (mantendo os valores numéricos)
- `RoleNames.Admin = "admin"` → `RoleNames.GestorONG` / `RoleNames.Doador`
- `JwtService.ObterClaims()`: hoje só `Administrador` recebe claim de role
  — ajustar para que **todo** usuário receba a claim correspondente

## O que NÃO existe ainda no repositório (cuidado ao assumir)

- Nenhum `Dockerfile` (só há `docker-compose.yml` para Postgres + PgAdmin —
  nada containeriza a própria API)
- Nenhum pipeline de CI (`.github/workflows` não existe)
- Nenhum manifest de Kubernetes
- RabbitMQ/MassTransit — zero referência no código ou nos pacotes
- Projeto `ConexaoSolidaria.Worker` — não existe, é um projeto novo a criar

## Banco de dados local

```
docker-compose up -d
# cria fcg-postgres (Postgres 18) e fcg-pgadmin
```

Connection string em `src/ConexaoSolidaria.API/appsettings.json` /
`appsettings.Development.json` (`ConnectionStrings:DefaultConnection`). O
`.env.example` na raiz e em `src/ConexaoSolidaria.API/` definem as credenciais do
`docker-compose` — ambos usam `conexao_solidaria` como nome do banco (a
versão copiada da Fase 1 tinha uma inconsistência entre `fcg_plataforma_jogos`
e `fiap_cloud_games`, já corrigida).

## Convenções

- Commits em português, registro informal-profissional
- Testes unitários: nome do método sempre começa com `Ao`, sem underline —
  ex. `AoCriarCampanhaComDataFimNoPassadoDeveLancarExcecao`
- SQL (quando houver stored procedure ou script fora do EF Core): palavras-
  chave em minúsculo, uma cláusula por linha, sem `select *`
- Login admin de teste (seed): `admin@conexaosolidaria.com.br` / `Admin@123`
  — válido também para `GestorONG` depois do rename de perfil
