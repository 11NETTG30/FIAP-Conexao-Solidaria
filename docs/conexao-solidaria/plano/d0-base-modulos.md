# D0 — Base e scaffolding

Responsável sugerido: **Gabriel**. É a demanda que **libera todo o resto** —
o Saulo está esperando isso subir para clonar e começar D1.

## Objetivo

Deixar o repositório pronto para os módulos `Campanha` e `Doacao` entrarem,
sem ainda implementar nenhuma regra de negócio deles.

## Instruções

Herda tudo de `visao-geral.md` e de
`../CONTEXTO-TECNICO.md`. Ler os dois antes de começar.

## Contexto

O repositório base tem hoje só o módulo `Identidade`, com um enum de perfil
genérico (`Usuario`/`Administrador`) que precisa virar os dois papéis reais
do domínio (`Doador`/`GestorONG`) antes que outros módulos referenciem essas
roles em seus `[Authorize]`.

## Andamento

- [x] Renomear `PerfilUsuario.Usuario` → `Doador`, `PerfilUsuario.Administrador`
      → `GestorONG` (mantendo os valores numéricos 1 e 2), propagando o rename
      em todo o código que referencia o enum
- [x] Ajustar `RoleNames` (`src/FCG.Infrastructure/Identidade/Security/`):
      trocar `Admin = "admin"` por `GestorONG` e `Doador`
- [x] Ajustar `JwtService.ObterClaims()`: hoje só `Administrador` recebia
      claim de role — agora todo usuário recebe a claim do seu perfil
      (`GestorONG` ou `Doador`)
- [x] Criar as pastas vazias dos módulos novos (com `.gitkeep`, já que Git
      não versiona pasta vazia):
      `src/FCG.Domain/Campanha/`, `src/FCG.Domain/Doacao/`,
      `src/FCG.Application/Campanha/`, `src/FCG.Application/Doacao/`,
      `src/FCG.Infrastructure/Campanha/`, `src/FCG.Infrastructure/Doacao/`
- [x] Rodar os testes existentes (`dotnet test FCG.Tests`) para confirmar
      que o rename não quebrou nada — 78 testes passando; também rodado
      `FCG.BDDTests` (10 passando) e `dotnet build` da solução inteira
      (0 erros)
- [x] Commit + push — enviado para `claude/brave-fermat-wnrmah` (branch de
      trabalho desta sessão); merge para a branch principal a cargo do
      grupo

## Notas de implementação

- `IInformacoesUsuarioLogado.Administrador` (bool) também foi renomeada para
  `GestorONG`, já que dependia de `RoleNames.Admin` (removida) e não tinha
  nenhum uso fora da própria classe — não é uma nova regra de negócio, só
  acompanha o rename do enum/roles para não deixar nome desalinhado com o
  domínio.
- `UsuarioController` (`[Authorize(Roles = ...)]`) e
  `TornarUsuarioAdministradorUseCase` (que promove um usuário a
  administrador) passaram a referenciar `RoleNames.GestorONG` /
  `PerfilUsuario.GestorONG` — mantive os nomes de classe/endpoint como estão,
  já que a demanda não pediu renomear esses símbolos, só o enum e as roles.
- A migration de seed (perfil `2`) não precisou de mudança nesta demanda: já
  grava o valor numérico do enum, que continua `2` para `GestorONG`. O e-mail
  do seed (`admin@fcg.com.br` → `admin@conexaosolidaria.com.br`) foi ajustado
  depois, numa auditoria de identidade separada (resíduos do boilerplate
  `fiap-cloud-games`), registrada em `visao-geral.md`.

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

Nenhum além do que já está referenciado acima.
