# Conexão Solidária — contexto para Claude Code

Este repositório implementa o MVP da plataforma "Conexão Solidária"
(Hackathon Fase 5, POSTECH .NET — Turma 11NETT, Grupo 30): gestão de
doadores e campanhas de arrecadação para uma ONG.

Monolito modular em Clean Architecture, .NET 10. **Antes de qualquer
mudança estrutural, leia:**

1. `docs/conexao-solidaria/ARQUITETURA.md` — arquitetura completa: domínio,
   agregados, banco, mensageria, auth
2. `docs/conexao-solidaria/CONTEXTO-TECNICO.md` — stack, estrutura de
   pastas, padrão de módulo (usar `Identidade` como referência), o que
   ainda não existe no repositório
3. `docs/conexao-solidaria/plano/visao-geral.md` — decisões tomadas,
   andamento geral, divisão de trabalho
4. `docs/conexao-solidaria/plano/<demanda>.md` — a demanda específica desta
   sessão (nome pedido na mensagem inicial, ex.: `d1-campanha.md`)

O edital completo do hackathon está em `edital/HACKATHON_11NETT.pdf`.

## Convenções

- Responder sempre em português
- Commits em português, registro informal-profissional
- Testes unitários: nome do método sempre começa com `Ao`, sem underline —
  ex. `AoCriarCampanhaComDataFimNoPassadoDeveLancarExcecao`
- SQL (quando houver stored procedure ou script fora do EF Core): palavras-
  chave em minúsculo, uma cláusula por linha, sem `select *`
- Ao retomar qualquer demanda, ler o `.md` dela em `docs/conexao-solidaria/plano/`
  antes de mexer em código, e atualizar o checklist "Andamento" dela ao
  longo do trabalho — não criar arquivos de anotação soltos para isso

## Origem do código

Copiado (sem o histórico de commits) de
`11NETTG30/fiap-cloud-games`, o boilerplate Clean Architecture + DDD da
Fase 1 do grupo. Peças prontas reaproveitáveis (Argon2id, RefreshToken
rotativo, RabbitMQ/MassTransit) estão em
`11NETTG30/fcg-users`, citadas nas demandas onde se aplicam — ambos
públicos, dá para consultar diretamente por URL.
