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

- [ ] Renomear `PerfilUsuario.Usuario` → `Doador`, `PerfilUsuario.Administrador`
      → `GestorONG` (mantendo os valores numéricos 1 e 2), propagando o rename
      em todo o código que referencia o enum
- [ ] Ajustar `RoleNames` (`src/FCG.Infrastructure/Identidade/Security/`):
      trocar `Admin = "admin"` por `GestorONG` e `Doador`
- [ ] Ajustar `JwtService.ObterClaims()`: hoje só `Administrador` recebe
      claim de role — todo usuário deve receber a claim do seu perfil
- [ ] Criar as pastas vazias dos módulos novos (com `.gitkeep`, já que Git
      não versiona pasta vazia):
      `src/FCG.Domain/Campanha/`, `src/FCG.Domain/Doacao/`,
      `src/FCG.Application/Campanha/`, `src/FCG.Application/Doacao/`,
      `src/FCG.Infrastructure/Campanha/`, `src/FCG.Infrastructure/Doacao/`
- [ ] Rodar os testes existentes (`dotnet test FCG.Tests`) para confirmar
      que o rename não quebrou nada
- [ ] Commit + push para a branch principal

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

Nenhum além do que já está referenciado acima.
