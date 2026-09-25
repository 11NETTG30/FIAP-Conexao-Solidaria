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

## Andamento

- [x] d0 — Base: rename de roles, scaffolding de pastas dos módulos novos
- [x] d1 — Módulo Campanha
- [ ] d2 — Módulo Doação + Worker + RabbitMQ
- [ ] d3 — Dockerfile + CI (build de imagem)
- [ ] d4 — Manifests Kubernetes
- [ ] d5 — Observabilidade (health/metrics + Grafana)
- [ ] d6 — Documentação final + vídeo de entrega

## Pendências / dúvidas

- Confirmar com o grupo se cache Redis de campanhas entra (era "se sobrar
  tempo" — não é bloqueante para nenhuma demanda abaixo).

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo do hackathon
- `../ARQUITETURA.md` — desenho técnico completo (domínio, agregados,
  módulos, banco, mensageria, auth, estrutura de pastas)
- `../CONTEXTO-TECNICO.md` — stack e padrão de módulo do repositório
