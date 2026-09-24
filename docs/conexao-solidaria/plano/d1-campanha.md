# D1 — Módulo Campanha

Responsável sugerido: **Saulo**. Depende de D0 já estar mergeado na branch
principal.

## Objetivo

Implementar o CRUD de campanhas e o painel público de transparência,
seguindo exatamente o padrão do módulo `Identidade` já existente.

## Instruções

Herda tudo de `visao-geral.md` e de
`../CONTEXTO-TECNICO.md` — **ler os dois antes de
começar**, especialmente a seção "Padrão de módulo — Identidade como
referência" do segundo. Não reinventar estrutura: espelhar
`src/FCG.Domain/Identidade`, `src/FCG.Application/Identidade`,
`src/FCG.Infrastructure/Identidade`.

## Contexto

Regras extraídas do edital (`../../../edital/HACKATHON_11NETT.pdf`):

- **Entidade Campanha**: `Titulo` (string), `Descricao` (string),
  `DataInicio` (datetime), `DataFim` (datetime), `MetaFinanceira` (decimal),
  `Status` (Ativa/Concluida/Cancelada), `ValorArrecadado` (decimal, inicia
  em 0)
- **Regra de negócio**: campanha não pode ser criada com `DataFim` no
  passado; `MetaFinanceira` deve ser maior que zero
- **`CampanhaDbContext`** com `SCHEMA = "campanha"` — seguir exatamente o
  padrão de `IdentidadeDbContext.cs`
- **Endpoints**:
  - `POST /campanhas` — só `GestorONG` — cria com `Status: Ativa`
  - `PUT /campanhas/{id}` — só `GestorONG` — edita
  - `GET /campanhas` — público, sem JWT — só `Status: Ativa`, retornando
    `Titulo`, `MetaFinanceira`, `ValorArrecadado` (não expor os demais
    campos a menos que o edital peça)
- **Não implementar** aqui a atualização do `ValorArrecadado` — isso é
  responsabilidade do `doacoes-worker` (demanda D2), que escreve direto no
  schema `campanha` numa transação própria. O módulo Campanha só expõe o
  campo para leitura.

Consulte `../ARQUITETURA.md`, seção "Agregados", para entender por
que Campanha não tem uma lista de Doações dentro dela.

## Decisões

- 2026-09-24 — Escopo: **só o obrigatório do edital**. Fora: trava de
  redução de meta/antecipação de data quando já há `ValorArrecadado`
  (estava no PDF antigo), paginação do `GET /campanhas`, cache Redis.
- 2026-09-24 — Namespace/pasta do módulo no **plural** (`Campanhas`), com a
  entidade no singular (`Campanha`). Com namespace `FCG.*.Campanha` e classe
  `Campanha`, o C# resolve o nome para o namespace antes da classe (erro
  CS0118) em qualquer arquivo fora de `Entities`. **O módulo Doação vai bater
  no mesmo problema** (`FCG.*.Doacao` + classe `Doacao`) — sugestão para a
  D2: usar `Doacoes`.
- 2026-09-24 — `Status` gravado como **texto** (`'Ativa'`, `'Concluida'`,
  `'Cancelada'`), não como int — para o worker filtrar com
  `status = 'Ativa'` direto no SQL, como está em `ARQUITETURA.md`.
- 2026-09-24 — Contrato para o worker (D2): tabela `campanha.campanhas`,
  colunas `id` (uuid), `status` (varchar), `valor_arrecadado`
  (numeric(18,2)). A edição via API só grava as colunas alteradas (change
  tracking do EF), então nunca sobrescreve o `valor_arrecadado` escrito pelo
  worker.
- 2026-09-24 — Regra "DataFim no passado" vale só na **criação** (como diz o
  edital); na edição valida apenas `DataFim > DataInicio`, para o gestor
  poder concluir/cancelar campanha já vencida. `Status` só é alterado pelo
  `PUT` (criação sempre nasce `Ativa`).
- 2026-09-24 — `PUT /campanhas/{id}` com **todos os campos opcionais**: o
  `EditarCampanhaUseCase` só chama o `SetX()` do campo que vier preenchido
  (`if (request.X is not null)`). Datas: se vier só uma, a outra mantém o
  valor atual da campanha e o par é validado junto (`DataFim > DataInicio`).
  O validator só aplica cada regra quando o campo vem preenchido.
- 2026-09-24 — `GET /campanhas` devolve também o `Id`, além de Titulo,
  MetaFinanceira e ValorArrecadado — o doador precisa dele para informar o
  `IdCampanha` na doação.
- 2026-09-24 — Rotas seguem o prefixo já usado no repositório:
  `api/campanhas`. Enums passaram a trafegar pelo nome no JSON
  (`JsonStringEnumConverter` global; números continuam aceitos).
- 2026-09-24 — Estágio `migrate` do `Dockerfile` passou a rodar
  `database update --context` para cada DbContext (variável
  `MIGRATION_CONTEXTS`) — com mais de um contexto o comando sem `--context`
  falha. Ao criar o `DoacaoDbContext`, acrescentar na variável.

## Andamento

- [x] Entidade `Campanha` (Domain) com validações de `DataFim` e
      `MetaFinanceira`
- [x] `CampanhaDbContext` + `IEntityTypeConfiguration<Campanha>`
- [x] Repositório `ICampanhaRepository` / `CampanhaRepository`
- [x] Casos de uso: `CriarCampanhaUseCase`, `EditarCampanhaUseCase`,
      `ListarCampanhasAtivasUseCase` (+ validators FluentValidation)
- [x] `CampanhaController` com os 3 endpoints
- [x] Registrar `CampanhaDbContext` em `DatabaseConfiguration.cs` e o
      repositório em `DependencyInjectionInfrastructure.cs` (+ use cases em
      `DependencyInjectionApplication.cs` e migrate on startup no
      `Program.cs`)
- [x] Testes unitários das regras de validação (nomenclatura `Ao...`, sem
      underline) — `FCG.Tests` 95 passando, `FCG.BDDTests` 10 passando
- [x] Migration inicial do schema `campanha`
- [ ] Teste ponta a ponta local (docker-compose: migrate + login admin +
      POST/PUT/GET) — pendente, Docker Desktop estava parado
- [x] Commit + push na branch `feature/d1-campanha` — divergências do texto
      original (prefixo `api/` nas rotas, `Id` no `GET`, pasta `Campanhas`)
      aceitas pelo grupo; PR para a branch principal ainda não aberto

## Pendências / dúvidas

- ~~Confirmar se o `GET /campanhas` deve paginar~~ — não pagina (fora do
  obrigatório, ver Decisões)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
- `../ARQUITETURA.md` — arquitetura geral
