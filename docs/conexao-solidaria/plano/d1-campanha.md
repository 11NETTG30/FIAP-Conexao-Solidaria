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
`src/ConexaoSolidaria.Domain/Identidade`, `src/ConexaoSolidaria.Application/Identidade`,
`src/ConexaoSolidaria.Infrastructure/Identidade`.

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
  entidade no singular (`Campanha`). Com namespace `ConexaoSolidaria.*.Campanha` e classe
  `Campanha`, o C# resolve o nome para o namespace antes da classe (erro
  CS0118) em qualquer arquivo fora de `Entities`. **O módulo Doação vai bater
  no mesmo problema** (`ConexaoSolidaria.*.Doacao` + classe `Doacao`) — sugestão para a
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

- 2026-09-25 — **Revisão pós-auditoria** (postura "levantar antes de
  assumir"): as decisões implícitas da D1 foram levadas ao Saulo em rodada
  de perguntas, todas resolvidas pela recomendação:
  - **Campanha vencida**: `DataFim` passou e status continua `Ativa` → sai
    do painel (`status = 'Ativa' and data_fim >= agora`). O worker aplica o
    mesmo filtro — SQL de referência atualizado em `ARQUITETURA.md`. Não há
    job mudando o status para `Concluida`.
  - **Transições de status**: `Concluida` e `Cancelada` são estados
    finais — qualquer `PUT` em campanha não `Ativa` → 400
    (`Campanha.GarantirQuePodeSerEditada()`), inclusive para reativar.
  - **Validação em duas camadas** (FluentValidation + entidade, padrão da
    Identidade) mantida. Corrigido bug compartilhado em
    `DependencyInjectionApplication.cs`: a cultura pt-BR era definida antes
    de trocar o `LanguageManager` e se perdia — mensagens saíam em inglês
    (afeta a Identidade também, para melhor).
  - **Campanha inexistente → 404**: nova `NotFoundException` em
    `Domain/Shared/Exceptions`, mapeada no `DomainExceptionMiddleware`.
    A Identidade continua usando 400 para usuário inexistente (não mexido).
  - **Datas em UTC**: data sem fuso é tratada como UTC; documentado no
    Swagger e no README.
  - **Casas decimais**: `MetaFinanceira` com mais de 2 casas → 400 (validator
    `PrecisionScale(18, 2)` + entidade), em vez de o Postgres arredondar em
    silêncio.
  - **Limites de texto** (título 3–150, descrição obrigatória até 2000) e
    `DataFim` estritamente posterior à `DataInicio`: mantidos.
  - **`JsonStringEnumConverter` global**: mantido.
  - **`POST /campanhas`** responde `201` com `{ "id": "..." }` (não mais o
    GUID puro). Painel segue ordenado por `DataFim` (as que terminam antes
    primeiro).
- 2026-09-25 — Rodada 2 da revisão (escopo reduzido de propósito: o foco do
  projeto é demonstrar a arquitetura, não esgotar regra de negócio):
  - **Painel só mostra campanha que já começou**: filtro completo é
    `status = 'Ativa' and data_inicio <= agora and data_fim >= agora`.
    Criação continua aceitando `DataInicio` no futuro (campanha agendada).
  - **Edição não pode mover `DataFim` para o passado**: a checagem saiu do
    construtor e foi para `SetPeriodo`, valendo na criação e na edição
    (mesma mensagem). Para encerrar, o caminho é `status: Concluida`.
  - **Não entra**: o worker **não** filtra `data_inicio` (só `data_fim`) —
    campanha agendada fica fora do painel, mas uma doação com o id dela
    seria aceita; e não há trava para mover a `DataInicio` de campanha já
    iniciada para o futuro. Ambos aceitos como fora do escopo.
- 2026-09-25 — Achado na revisão: o README mandava `Update-Database` sem
  `-Context`, que falha desde que existem dois DbContexts — corrigido para
  um comando por contexto.

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
- [x] Teste ponta a ponta local (docker-compose: migrate + login admin +
      POST/PUT/GET) — 2026-09-24, 27 cenários OK: migrate aplica as duas
      migrations (uma por contexto); 401 sem token, 403 para `Doador`;
      DataFim no passado / meta 0 / DataFim < DataInicio → 400; painel só
      com Ativas e só os 4 campos; PUT parcial altera só o campo enviado;
      PUT não sobrescreve `valor_arrecadado` incrementado por SQL (simulando
      o worker); status aceito por nome e por número; cancelada some do
      painel
- [x] Commit + push na branch `feature/d1-campanha` — divergências do texto
      original (prefixo `api/` nas rotas, `Id` no `GET`, pasta `Campanhas`)
      aceitas pelo grupo; PR #5 mergeado na `main`
- [x] Revisão pós-auditoria (ver Decisões de 2026-09-25): ajustes
      implementados, `FCG.Tests` 104 passando, `FCG.BDDTests` 10 passando,
      teste ponta a ponta local com 23 cenários OK (inclui 404, estados
      finais, campanha vencida fora do painel, mensagens em pt-BR, `{ id }`
      no POST)
- [x] Rodada 2 da revisão: painel filtra `DataInicio <= agora`; edição não
      move `DataFim` para o passado — `FCG.Tests` 106 passando, ponta a
      ponta com 28 cenários OK

## Pendências / dúvidas

- ~~Rodada 2 da revisão~~ — resolvida, ver Decisões de 2026-09-25.
- **Alinhar com o Gabriel (D2)** — decisões desta demanda que tocam o
  worker/módulo Doação:
  - Worker precisa filtrar `data_fim >= now()` além de `status = 'Ativa'`
    (SQL de referência já atualizado em `ARQUITETURA.md`). **Não** precisa
    filtrar `data_inicio` (decidido fora do escopo, apesar de o painel
    filtrar)
  - `ValorDoacao` com o mesmo tipo do `valor_arrecadado`: `numeric(18,2)`,
    rejeitando mais de 2 casas decimais
  - `NotFoundException` (→ 404) disponível no Shared para o
    `GET /doacoes/{id}`
  - Enums trafegam pelo nome no JSON (o `Status` da Doação vai sair como
    `"Pendente"`)
  - Mensagens do FluentValidation agora saem em pt-BR para todos os módulos
  - Formato de resposta de criação: `{ "id": "..." }` — sugestão de usar o
    mesmo no `POST /doacoes`

- ~~Confirmar se o `GET /campanhas` deve paginar~~ — não pagina (fora do
  obrigatório, ver Decisões)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
- `../ARQUITETURA.md` — arquitetura geral
