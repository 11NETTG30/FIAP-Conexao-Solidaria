# Visão geral — Conexão Solidária

Este arquivo dá o panorama do projeto inteiro. As demandas individuais estão
em `d0-base-modulos.md`, `d1-campanha.md` etc., nesta mesma pasta.

## Objetivo

Arquitetar e entregar o MVP da plataforma **Conexão Solidária** — gestão de
doadores e campanhas de arrecadação para a ONG Esperança Solidária — atendendo
ao edital do Hackathon (`../../../edital/HACKATHON_11NETT.pdf`) dentro do prazo do
grupo.

## Instruções

- Responder sempre em português.
- Seguir as convenções de código já registradas em
  `../CONTEXTO-TECNICO.md` — não reinventar padrão que já
  existe no repositório (nomes de método de teste com prefixo "Ao", sem
  underline; SQL em minúsculo quando houver stored procedure envolvida).
- Ao retomar qualquer demanda desta árvore, ler o `.md` dela em `plano/` antes de
  mexer em código, e registrar decisões e andamento nela mesma — não criar
  arquivos novos soltos para isso.
- Este projeto tem várias pessoas trabalhando em paralelo (ver cada
  sub-demanda). Não assumir posse de módulo que não seja o da demanda aberta.

## Contexto

O edital completo está em `../../../edital/HACKATHON_11NETT.pdf`. Resumo do que ele
exige:

- Autenticação JWT com dois perfis: `GestorONG` e `Doador`
- Gestão de campanhas (criar/editar) — só `GestorONG`
- Cadastro público de doadores
- Painel de transparência público (campanhas ativas + valor arrecadado)
- Processo de doação: o doador envia uma intenção; a atualização do valor
  **não pode ser direta no banco** na mesma requisição
- Pelo menos 2 microsserviços distintos
- Comunicação assíncrona via broker (RabbitMQ/Kafka) para processar doações
- Orquestração em Kubernetes (yaml de Deployments/Services/ConfigMaps)
- Observabilidade (Zabbix/Grafana com métricas reais)
- Pipeline de CI/CD (build + imagem Docker a cada push)

A arquitetura completa (domínio, agregados, módulos, banco, mensageria,
auth) está detalhada em `../ARQUITETURA.md` — leitura obrigatória antes
de qualquer demanda filha.

## Decisões

- 2026-09-19 — Desenho inicial: 3 bounded contexts isolados (Identidade,
  Campanhas, Doações), cada um com repositório, deploy e banco próprios, Kong
  como API Gateway com plugin `jwt-keycloak`. **Superado pela decisão
  seguinte.**
- 2026-09-20 — Grupo decide simplificar drasticamente, dado o tempo
  disponível: **1 repositório único**, **2 serviços** (API com todos os
  módulos organizados em pastas + Worker que processa doações), **sem API
  Gateway** (Kong cai fora — era opcional), **3 schemas num único banco
  Postgres** (não 3 databases separados — decisão tomada aqui mesmo, nesta
  conversa, especificamente para permitir que o Worker atualize
  Campanha e Doação numa única transação local, sem precisar de outbox
  pattern).
- 2026-09-20 — Confirmado: Campanha e Doação continuam como **dois agregados
  separados** (não "Doação como lista dentro de Campanha") — o próprio fluxo
  assíncrono exigido pelo edital já pressupõe duas fronteiras de consistência
  distintas, e o volume de doações por campanha inviabilizaria um agregado
  único.
- 2026-09-20 — Worker acessa o banco diretamente (schemas `campanha` e
  `doacao`, mesma instância Postgres) em vez de fazer requisição HTTP para
  outra API — elimina o salto de rede entre serviços, mantendo a fila como
  gatilho assíncrono (exigência do edital), não como ponte entre bancos
  fisicamente separados.
- 2026-09-24 — Nome do worker definido como `doacoes-worker` (documentação de
  intenção — consome `DoacaoRecebidaEvent`, atualiza Campanha E marca a
  Doação como Confirmada/Rejeitada na mesma transação).
- 2026-09-24 — Base de código escolhida: partir do repositório
  `fiap-cloud-games` (monolito da Fase 1), não do `fcg-users` (microsserviço
  extraído para o desenho antigo com Kong/RS256). Ver justificativa completa
  em `../CONTEXTO-TECNICO.md`.
- 2026-09-24 — Divisão de trabalho: Campanha (d1) com o Saulo, Doação +
  Worker + RabbitMQ (d2) com o Gabriel, em paralelo assim que a base (d0)
  subir.
- 2026-09-24 — Repositório criado: `github.com/11NETTG30/FIAP-Conexao-Solidaria`
  (privado). Populado a partir do fork `github.com/11NETTG30/fiap-cloud-games`,
  **sem** o histórico de commits da Fase 1 (squash num único commit raiz,
  para não misturar mensagens/datas de outro desafio na avaliação deste).
  Os repositórios `fiap-cloud-games` e `fcg-users` (ambos públicos) continuam
  existindo só como referência de leitura, não são o repositório de trabalho.
- 2026-09-24 — Repositório transferido de `gaabrielalex/FIAP-Conexao-Solidaria`
  (conta pessoal) para `11NETTG30/FIAP-Conexao-Solidaria` (organização) — mesmo
  histórico de commits, só mudou o dono. Referências a `gaabrielalex/fiap-cloud-games`
  e `gaabrielalex/fcg-users` atualizadas para `11NETTG30/fiap-cloud-games` e
  `11NETTG30/fcg-users` em todo o repositório, já que agora dá pra referenciar os
  repositórios originais direto em vez dos forks pessoais.
- 2026-09-24 — Auditoria de identidade do projeto antigo (boilerplate
  `fiap-cloud-games`, plataforma de jogos): corrigidos README (reescrito do
  zero), metadados do Swagger/OpenAPI, `JwtSettings:Issuer`/`Audience`
  (`FCG.API`/`FCG.Client` → `ConexaoSolidaria.API`/`ConexaoSolidaria.Client`
  em `appsettings.json`, `.env.example`, `docker-compose.yml` e `render.yaml`),
  e-mail do usuário admin de seed (`admin@fcg.com.br` →
  `admin@conexaosolidaria.com.br`), e nomes de container/rede do
  `docker-compose.yml` (`fcg-*` → `conexao-solidaria-*`). Namespaces `FCG.*`,
  nome da solução (`FiapCloudGames.slnx`) e o usuário do Postgres em
  `render.yaml` (`fcgadmin`) **não** foram tocados — mudança maior, decisão do
  Gabriel, ver ressalvas na sessão que fez esta auditoria.
- 2026-09-25 — Segunda varredura por termos residuais (`game`/`games`/`cloud`/
  `FCG`) fora de `tests/`. Corrigidos mais dois resíduos de baixo risco:
  `newrelic.config` (`<name>FiapCloudGames</name>` → `ConexaoSolidaria`) e
  `FCG.API.http` (request de exemplo apontava pra `/Usuario/Teste`, endpoint
  que não existe mais — trocado por `POST /api/auth/login` com o admin de
  seed). Decisão explícita do Gabriel: **adiar** o rename dos namespaces
  `FCG.*`/`FiapCloudGames.slnx` e o `user: fcgadmin` do `render.yaml` — D2
  (Doação + Worker) está em andamento em paralelo e mexer nisso agora geraria
  conflito de merge; revisitar depois que `tests/FCG.Tests` também sair (ou
  quando o grosso das demandas estiver mergeado, o que vier primeiro).
- 2026-09-25 — Gabriel removeu `tests/FCG.BDDTests` inteiro direto no `main`
  (decidiu manter só `FCG.Tests`/xUnit — ajuda no desenvolvimento assistido
  por IA). Esse commit não atualizou o `FiapCloudGames.slnx`, que ficou
  referenciando o projeto apagado; corrigido (tirada a entrada do BDD do
  `.slnx`), junto com as menções a BDD/Reqnroll/NUnit em `README.md` e
  `CONTEXTO-TECNICO.md` (tabela de stack e diagrama de pastas) e os e-mails
  de exemplo `@fcg.com.br` que sobraram em `UsuarioTests.cs` (agora
  `@conexaosolidaria.com.br`) — `tests/FCG.Tests` continua no repositório,
  então passou a valer a mesma limpeza de identidade do resto do código.
- 2026-09-25 — Rename dos namespaces `FCG.*` → `ConexaoSolidaria.*` (item que
  vinha represado desde a auditoria de identidade): pastas, `.csproj` e
  `.slnx` renomeados via `git mv` (`FiapCloudGames.slnx` →
  `ConexaoSolidaria.slnx`), `namespace`/`using` de todo o código-fonte
  atualizados, `Dockerfile` ajustado. Atualizadas também as referências vivas
  em `ARQUITETURA.md`, `CONTEXTO-TECNICO.md`, `README.md` e nos planos
  `d1`–`d5` (inclusive `ConexaoSolidaria.Worker`, ainda não criado, na
  demanda D2) — checklists "Andamento" já fechados (`d0`, e os registros de
  contagem de teste em `d1`) mantidos como estavam, por serem registro
  histórico do nome válido na época. `render.yaml` (`user: fcgadmin`)
  continua de fora — não fazia parte deste item.
- 2026-09-27 — Auditoria edital x demandas (a pedido do Gabriel): comparado
  `edital/HACKATHON_11NETT.pdf` item a item contra d0–d6. Achado um requisito
  funcional obrigatório sem demanda associada — **CPF do doador** (item 3 do
  edital, "validar formato"), ausente da entidade `Usuario` (herdada do
  `fiap-cloud-games`, que nunca teve esse campo). Registrado como **d0.1**
  (`d0.1-cpf-doador.md`), adendo à própria D0/Identidade (do Gabriel), não
  como demanda nova no fim da fila — confirmado por busca no repositório que
  `new Usuario(...)` só é usado dentro do próprio módulo Identidade; D1/D2
  referenciam usuário só pelo `Guid` (`IdDoador`), nunca a entidade. Não
  impacta o andamento de d1–d5; a única dependência real é fechar antes de a
  d6 travar o README e o roteiro do vídeo (o script `e2e/smoke-test.sh` e
  qualquer coleção Postman/prints do Swagger usados ali ficariam
  desatualizados se o CPF entrar depois).
- 2026-09-28 — Sincronizado com PR #17 (`feat/observabilidade`), mergeado
  pelo Lennon: trouxe d3 (workflow `.github/workflows/docker-publish.yml`,
  build+test+push das duas imagens pro GHCR a cada push na `main`) e d5
  (OpenTelemetry + Prometheus + Grafana, validados rodando de verdade num
  cluster Kubernetes do Docker Desktop) praticamente prontas, além dos
  manifests da d4 (`k8s/`) completos — só falta a d4 confirmar formalmente
  o `kubectl apply -f k8s/` ponta a ponta no seu próprio checklist. D6
  (`d6-documentacao-final.md`) detalhada em blocos por dependência, a
  pedido do Gabriel, para dividir o trabalho entre o grupo: só o Bloco B
  (README de doação + roteiro de vídeo de autenticação/campanha/doação)
  segue bloqueado, esperando a mesma validação ponta a ponta da d2 que já
  estava pendente.
- 2026-09-28 — Ao validar a d0.1 ponta a ponta, achado um bug pré-existente
  em `e2e/smoke-test.sh`, sem relação com CPF: o `.env` temporário gerado
  pelo próprio script nunca ganhou `RABBITMQ_USER`/`RABBITMQ_PASSWORD`
  quando o RabbitMQ entrou no projeto (d2) — sem essas variáveis,
  `RabbitMqSettings.Username`/`Password` ficam vazios e o container `api`
  derruba na subida (`OptionsValidationException`), fazendo toda a bateria
  do smoke test falhar por "conexão recusada", não só os cenários de auth.
  Corrigido (duas linhas a mais no `.env` gerado). Registrado aqui porque
  não é escopo da d0.1 e pode ser relevante para quem for fechar a
  validação ponta a ponta pendente da d2/d4 — com o fix, o smoke test
  passou 36/36 numa stack com Postgres + RabbitMQ + API reais via
  docker-compose (só não cobre o fluxo de doação/worker em si, que não faz
  parte do `smoke-test.sh`).

## Andamento

- [x] d0 — Base: rename de roles, scaffolding de pastas dos módulos novos
- [x] d0.1 — CPF do doador (Gabriel; ver `d0.1-cpf-doador.md`) — validado
      ponta a ponta (`dotnet test` + `e2e/smoke-test.sh` 36/36), PR #18
      mergeado
- [x] d1 — Módulo Campanha
- [x] d2 — Módulo Doação + Worker + RabbitMQ (fechada por decisão do
      Gabriel em 2026-09-28 sem a validação ponta a ponta via
      docker-compose ter rodado de fato — ver `d2-doacao-worker.md`)
- [x] d3 — Dockerfile + CI (build de imagem, PR #17 traz também o workflow
      de publicação no GHCR)
- [ ] d4 — Manifests Kubernetes (todos os `.yaml` prontos; falta só marcar
      formalmente o `kubectl apply -f k8s/` ponta a ponta — d5 relata ter
      validado tudo rodando junto num cluster Docker Desktop, então isso
      pode já estar coberto na prática, só não atualizado no checklist da
      d4)
- [x] d5 — Observabilidade (health/metrics + Grafana) — validada rodando de
      verdade no Kubernetes do Docker Desktop em 2026-09-27
- [ ] d6 — Documentação final + vídeo de entrega

## Pendências / dúvidas

- Confirmar com o grupo se cache Redis de campanhas entra (era "se sobrar
  tempo" — não é bloqueante para nenhuma demanda abaixo).
- `../ARQUITETURA.md` (seção "Estrutura de pastas") referencia
  `implementacao/fcg-monolito/IMPLEMENTACAO.md`, arquivo que não existe no
  repositório — link quebrado, encontrado na varredura de 2026-09-25;
  precisa de alguém que saiba qual era o conteúdo/destino pretendido para
  corrigir.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo do hackathon
- `../ARQUITETURA.md` — desenho técnico completo (domínio, agregados,
  módulos, banco, mensageria, auth, estrutura de pastas)
- `../CONTEXTO-TECNICO.md` — stack e padrão de módulo do repositório
