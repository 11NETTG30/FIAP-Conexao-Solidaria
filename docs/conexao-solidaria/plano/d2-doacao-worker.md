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
- **`POST /doacoes`** (Doador autenticado): valida `ValorDoacao > 0`, salva
  `Status: Pendente`, publica `DoacaoRecebidaEvent` (`IdDoacao`,
  `IdCampanha`, `ValorDoacao`) via MassTransit/RabbitMQ. **Não** valida o
  status da campanha aqui — isso é responsabilidade do Worker, que é a
  fonte da verdade
- **`GET /doacoes/{id}`** (Doador autenticado, só o dono pode consultar)
- **RabbitMQ + MassTransit**: o repositório ainda não tem nenhum dos dois
  configurados — adicionar pacotes, configurar no `Program.cs`, subir um
  serviço `rabbitmq` no `docker-compose.yml`
- **Projeto `FCG.Worker`** (novo, Worker Service .NET, adicionado à
  solution): consome `DoacaoRecebidaEvent` e, numa única transação Postgres
  cobrindo os schemas `campanha` e `doacao` (ver SQL de exemplo em
  `../ARQUITETURA.md`):
  1. Busca a Campanha, valida `Status: Ativa`
  2. Se válido: incrementa `ValorArrecadado`, marca a Doacao como
     `Confirmada`
  3. Se inválido (campanha encerrada/cancelada): marca a Doacao como
     `Rejeitada`
  4. **Idempotência**: antes de aplicar, checar numa tabela de controle
     (`doacao.doacoes_processadas`) se o `IdDoacao` já foi processado — o
     broker pode reentregar mensagens

## Decisões

(decisões de arquitetura ficam no `visao-geral.md` — ver especialmente as
entradas de 2026-09-20 sobre banco único e fila)

## Andamento

- [ ] Entidade `Doacao` (Domain)
- [ ] `DoacaoDbContext` + configuração da entidade + tabela
      `doacoes_processadas`
- [ ] Repositório `IDoacaoRepository` / `DoacaoRepository`
- [ ] Adicionar RabbitMQ ao `docker-compose.yml`
- [ ] Configurar MassTransit no `Program.cs` da API (publisher) e do Worker
      (consumer)
- [ ] Caso de uso `RegistrarIntencaoDoacaoUseCase` (publica o evento)
- [ ] `DoacaoController` com os 2 endpoints
- [ ] Criar projeto `FCG.Worker` na solution
- [ ] Consumer `DoacaoRecebidaEventConsumer` com a transação cross-schema +
      idempotência
- [ ] Registrar `DoacaoDbContext` em `DatabaseConfiguration.cs` e o
      repositório em `DependencyInjectionInfrastructure.cs`
- [ ] Testes unitários (regra de validação de valor, idempotência do
      consumer)
- [ ] Migration inicial do schema `doacao`

## Pendências / dúvidas

- Confirmar nome exato da fila/exchange no RabbitMQ (sugestão:
  `doacao-recebida`, seguindo convenção do MassTransit)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
- `../ARQUITETURA.md` — arquitetura geral, com o SQL de referência
  da transação cross-schema
