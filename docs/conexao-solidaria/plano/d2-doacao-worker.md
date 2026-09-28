# D2 — Módulo Doação + Worker + RabbitMQ

Responsável sugerido: **Gabriel**. Depende de D0. Pode rodar em paralelo com
D1 (módulos diferentes), mas o Worker desta demanda escreve no schema
`campanha` — coordenar com quem estiver em D1 se os dois avançarem ao mesmo
tempo, para não haver conflito na entidade `Campanha` (leitura/escrita
convivem sem problema; só a *definição* da entidade não pode divergir entre
as duas demandas).

## Objetivo

Implementar o recebimento de doações, a fila assíncrona e o processamento
que atualiza o valor arrecadado da campanha.

## Instruções

Herda tudo de `visao-geral.md` e de
`../CONTEXTO-TECNICO.md`. Ler também
`../ARQUITETURA.md` inteiro antes de começar — esta demanda é a que
mais depende de entender o *porquê* das decisões (fila + banco único +
transação cross-schema), não só o *o quê*.

## Contexto

Regras extraídas do edital e da arquitetura combinada:

- **Entidade Doacao**: `IdCampanha` (referência por ID), `IdDoador`,
  `ValorDoacao` (decimal), `Status` (Pendente/Confirmada/Rejeitada)
- **`DoacaoDbContext`** com `SCHEMA = "doacao"`
- **`POST /doacoes`** (Doador autenticado): valida `ValorDoacao > 0` e faz
  uma checagem **não-autoritativa** de que a campanha existe e está apta
  (`PodeReceberDoacao()`), salva `Status: Pendente`, publica
  `DoacaoRecebidaEvent` (`IdDoacao`, `IdCampanha`, `ValorDoacao`) via
  MassTransit/RabbitMQ. O Worker continua sendo a fonte da verdade — a
  checagem da API é só "fail-fast", a campanha pode mudar de status entre
  o POST e o processamento assíncrono (ver "Decisões")
- **`GET /doacoes/{id}`** (Doador autenticado, só o dono pode consultar)
- **RabbitMQ + MassTransit**: o repositório ainda não tem nenhum dos dois
  configurados — adicionar pacotes, configurar no `Program.cs`, subir um
  serviço `rabbitmq` no `docker-compose.yml`
- **Projeto `ConexaoSolidaria.Worker`** (novo, Worker Service .NET, adicionado à
  solution): consome `DoacaoRecebidaEvent` e, numa única transação Postgres
  cobrindo os schemas `campanha` e `doacao` (ver SQL de exemplo em
  `../ARQUITETURA.md`):
  1. Incrementa `ValorArrecadado` da Campanha atomicamente via
     `ExecuteUpdateAsync`, com a condição `Status: Ativa` + dentro do prazo
     na mesma instrução (sem carregar a entidade em memória) — ver
     "Decisões"
  2. Se a instrução afetou 1 linha (campanha estava apta): marca a Doacao
     como `Confirmada`
  3. Se afetou 0 linhas (campanha encerrada/cancelada/inexistente): marca a
     Doacao como `Rejeitada`
  4. **Idempotência**: antes de aplicar, checar numa tabela de controle
     (`doacao.doacoes_processadas`) se o `IdDoacao` já foi processado — o
     broker pode reentregar mensagens

## Decisões

(decisões de arquitetura ficam no `visao-geral.md` — ver especialmente as
entradas de 2026-09-20 sobre banco único e fila)

- **MassTransit fixado em 8.5.10** (não 9.x): a partir da v9, o MassTransit
  passou a exigir licença comercial (`MT_LICENSE`/`SetLicense`) mesmo para
  uso local — confirmado ao rodar a API, que lançava
  `MassTransit.ConfigurationException` na inicialização. Como o projeto é
  um hackathon sem orçamento, fixamos a última versão totalmente open
  source (Apache-2.0), a 8.5.10, em
  `ConexaoSolidaria.Infrastructure.csproj`. Não atualizar para 9.x sem
  revisar a licença antes.
- **Nome da fila/endpoint**: `doacao-recebida`, configurado explicitamente
  via `ReceiveEndpoint("doacao-recebida", ...)` no Worker (em vez de
  depender do nome de convenção padrão do MassTransit), respondendo à
  dúvida em aberto abaixo.
- **`GET /doacoes/{id}` para não-dono retorna 404 (não 401/403)**: decisão
  deliberada para não vazar a existência de uma doação de outro usuário —
  `ObterDoacaoPorIdUseCase` lança `NotFoundException` tanto para
  "não existe" quanto para "existe mas não é do chamador".
- **Transação cross-schema no Worker via conexão Npgsql compartilhada**:
  `UseTransactionAsync` do EF Core só funciona entre dois `DbContext`
  diferentes se ambos usarem a *mesma instância* de `DbConnection`. Por
  isso, no `Worker/Program.cs`, registramos uma única `NpgsqlConnection`
  com escopo por request e configuramos tanto `CampanhaDbContext` quanto
  `DoacaoDbContext` para usá-la (`UseNpgsql(sharedConnection, ...)`) — isso
  é específico do Worker; a API continua usando conexões por pool, uma por
  DbContext, sem mudança.
- **Verificação via `docker-compose` não foi possível neste ambiente**:
  o sandbox usado para a implementação não tem Docker instalado nem
  Postgres/RabbitMQ locais alcançáveis. Como alternativa, validamos que a
  API e o Worker sobem corretamente com `dotnet run` (todas as
  configurações de DI, EF Core e MassTransit resolvidas com sucesso),
  falhando apenas na conexão com o RabbitMQ por não haver um broker rodando
  localmente (erro esperado: `BrokerUnreachableException` /
  connection refused em `127.0.0.1:5672`). Recomenda-se rodar
  `docker-compose up` num ambiente com Docker (ou em CI) para validar o
  fluxo ponta a ponta antes do merge.
- **Incremento de `ValorArrecadado` atômico via `ExecuteUpdateAsync`**: a
  primeira versão do consumer carregava a `Campanha` em memória
  (`FindAsync` + `IncrementarValorArrecadado` + `SaveChanges`), o que é uma
  corrida real — dois workers processando doações da mesma campanha ao
  mesmo tempo podem ler o mesmo valor antigo e um `SaveChanges` sobrescreve
  o incremento do outro. Trocado por
  `Campanhas.Where(...).ExecuteUpdateAsync(s => s.SetProperty(c =>
  c.ValorArrecadado, c => c.ValorArrecadado + valor)...)`, que gera
  `SET valor_arrecadado = valor_arrecadado + :valor` no Postgres — soma
  atômica no próprio banco, sem SELECT prévio. A condição
  (`Status: Ativa` + `DataFim >= now()`) roda no `WHERE` da mesma
  instrução, e o número de linhas afetadas (0 ou 1) decide
  Confirmada/Rejeitada — substitui também o antigo carregamento da
  Campanha só para checar `PodeReceberDoacao()`. Como `ExecuteUpdateAsync`
  não passa pelo change tracker, o `AuditoriaSaveChangesInterceptor` não
  atualiza `DataAtualizacao` sozinho nesse caminho — por isso o
  `SetProperty(c => c.DataAtualizacao, ...)` é explícito na mesma chamada.
  O método `Campanha.IncrementarValorArrecadado()` ficou sem uso em
  produção depois dessa mudança e foi removido do domínio (junto com seus
  testes unitários) — decisão do time: sem uso real, não vale manter só
  como API de domínio "documental".
- **Validação do status da campanha em dois lugares (API e Worker)**: a
  API faz uma checagem não-autoritativa (fail-fast, 404/409 imediato para
  campanha inexistente/inapta) via `ICampanhaRepository.ObterPorId`:
  `PodeReceberDoacao()`; o Worker continua sendo a fonte da verdade
  (checagem atômica dentro do `ExecuteUpdateAsync`, ver decisão acima),
  porque a campanha pode mudar de status entre o POST e o processamento
  assíncrono da fila.
- **Outbox transacional do MassTransit só na API** (`AddEntityFrameworkOutbox<DoacaoDbContext>`
  com `UseBusOutbox()`, tabelas `OutboxMessage`/`OutboxState` adicionadas
  ao `DoacaoDbContext`): resolve um problema que não tinha solução antes —
  `RegistrarIntencaoDoacaoUseCase` fazia `Commit()` (grava a Doacao) e só
  depois `Publicar()` (publica no RabbitMQ) como duas operações separadas;
  se o `Publish` falhasse depois do `Commit` ter sucesso, a doação ficava
  presa em `Pendente` para sempre, sem nenhum evento disparado. Com o
  outbox, a ordem virou `Publicar()` (enfileira na tabela de outbox, não
  envia ainda) seguido de `Commit()` (grava Doacao + outbox na mesma
  transação); o serviço de entrega do MassTransit publica de fato só
  depois do commit ter sucesso. Efeito colateral aceito: como
  `AddOutboxMessageEntity()` inclui uma FK opcional para `InboxState`, a
  migration também criou a tabela `InboxState` no schema `doacao`, mesmo
  sem usá-la (ver próxima decisão) — tabela vazia, sem custo funcional.
- **Mantida a tabela `doacao.doacoes_processadas` no Worker, em vez do
  `InboxState`/dedup nativo do MassTransit EF Outbox**: avaliado usar
  `UseEntityFrameworkOutbox<DoacaoDbContext>()` no consumer para
  deduplicar via `InboxState` (chave por `MessageId` do broker) no lugar da
  tabela manual. Decisão: manter `doacoes_processadas` por ora —
  deduplicar pelo `IdDoacao` (regra de negócio) é mais robusto que
  deduplicar pelo `MessageId` do transporte (um reprocessamento manual/
  republish com novo `MessageId` para o mesmo `IdDoacao` não seria pego
  pelo `InboxState`, mas é pego pela tabela atual); além disso, não foi
  validado se o Inbox do MassTransit convive sem atrito com a transação
  cross-schema manual (`BeginTransactionAsync` + `UseTransactionAsync`
  compartilhada entre `CampanhaDbContext` e `DoacaoDbContext`) já usada
  aqui. Reavaliar se surgir um segundo evento/consumer que justifique
  investir no padrão genérico.
- **Retry + dead-letter queue no Worker**: adicionado
  `endpoint.UseMessageRetry(r => r.Intervals(5s, 30s, 2min))` no
  `ReceiveEndpoint("doacao-recebida", ...)` — falhas transitórias (ex.:
  Postgres reiniciando) são reprocessadas antes de desistir. A DLQ já
  existe por convenção do MassTransit sem configuração extra: mensagens
  que esgotarem as tentativas de retry vão para a fila
  `doacao-recebida_error` (durável, criada automaticamente).
- **`DatabaseConfiguration.ResolveConnectionString` tornado público** (era
  privado com um método público `GetConnectionString` só delegando para
  ele) — simplificação, sem necessidade de duas assinaturas fazendo a
  mesma coisa. O Worker (`Program.cs`) agora chama
  `DatabaseConfiguration.ResolveConnectionString` diretamente.
- **`UserSecretsId` removido de `ConexaoSolidaria.Worker.csproj`**: era
  deixado pelo template padrão do `dotnet new worker` e não é usado (nem o
  Worker nem nenhum outro projeto do repositório usa User Secrets — todos
  usam `DotNetEnv` + `appsettings`), removido por consistência.
- **Dockerfiles separados por projeto**: o `Dockerfile` único na raiz (com
  estágio `final` para a API e `worker` para o Worker) foi trocado por
  `src/ConexaoSolidaria.API/Dockerfile` (mantém também o estágio `migrate`)
  e `src/ConexaoSolidaria.Worker/Dockerfile`, cada um dono do seu próprio
  build — `docker-compose.yml` e `render.yaml` foram atualizados para os
  novos caminhos. **Isso muda uma decisão registrada em `d3-docker-ci.md`**
  (D3, de outro responsável) — atualizado lá também, junto com uma nota
  avisando que o build com Docker não pôde ser revalidado nesta sessão.
- **`DatabaseConfiguration.ResolveConnectionString` com senha mascarada
  (valor fixo) no branch `PGHOST`**: não é um bug, é política de
  segurança da empresa para não expor senhas de banco em texto plano para
  agentes de IA/ferramentas de análise automatizada. Comportamento
  intencional, mantido como está.

## Andamento

- [x] Entidade `Doacao` (Domain)
- [x] `DoacaoDbContext` + configuração da entidade + tabela
  `doacoes_processadas`
- [x] Repositório `IDoacaoRepository` / `DoacaoRepository`
- [x] Adicionar RabbitMQ ao `docker-compose.yml`
- [x] Configurar MassTransit no `Program.cs` da API (publisher) e do Worker
  (consumer)
- [x] Caso de uso `RegistrarIntencaoDoacaoUseCase` (publica o evento;
  valida campanha de forma não-autoritativa)
- [x] `DoacaoController` com os 2 endpoints
- [x] Criar projeto `ConexaoSolidaria.Worker` na solution
- [x] Consumer `DoacaoRecebidaEventConsumer` com a transação cross-schema +
  idempotência + incremento atômico (`ExecuteUpdateAsync`)
- [x] Registrar `DoacaoDbContext` em `DatabaseConfiguration.cs` e o
  repositório em `DependencyInjectionInfrastructure.cs`
- [x] Testes unitários (regra de validação de valor, validação de campanha
  na API, idempotência do consumer)
- [x] Migration inicial do schema `doacao` + migration do outbox do
  MassTransit (`OutboxMessage`/`OutboxState`/`InboxState`)
- [x] Outbox transacional do MassTransit na API (`AddEntityFrameworkOutbox`)
- [x] Retry policy + DLQ no Worker (`UseMessageRetry`, fila `_error`)
- [x] Dockerfiles separados por projeto (API e Worker)

## Pendências / dúvidas

- ~~Confirmar nome exato da fila/exchange no RabbitMQ~~ — resolvido, ver
  "Decisões" (`doacao-recebida`).
- ~~Validação ponta a ponta via `docker-compose` ainda não foi executada~~
  — 2026-09-28: decisão explícita do Gabriel de considerar a demanda
  fechada sem essa validação formal. **Registro importante**: isso é uma
  decisão de prosseguir, não um teste que rodou — ninguém confirmou de
  fato, batendo em Postgres/RabbitMQ reais, que a API publica o evento, o
  Worker consome e a transação cross-schema funciona. Continua valendo a
  recomendação original: se der para rodar isso em algum momento antes da
  entrega (localmente ou via CI), é o tipo de coisa que só aparece com a
  infra de verdade rodando — e o roteiro do vídeo (D6) exige mostrar
  exatamente esse fluxo (payload → fila → valor atualizado), então a prova
  vai precisar existir de qualquer jeito na hora de gravar.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
- `../ARQUITETURA.md` — arquitetura geral, com o SQL de referência
  da transação cross-schema
