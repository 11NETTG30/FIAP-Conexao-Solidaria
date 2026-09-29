# Conexão Solidária — Arquitetura (Fase 5)

> Este documento substitui qualquer versão anterior em PDF. Se você encontrar
> um `Conexao_Solidaria_Arquitetura.pdf` por aí, ele descreve o desenho
> **antigo** (3 repositórios, Kong, bounded contexts isolados) e está
> desatualizado. Esta é a versão corrente.

## Domínio e subdomínios

**Domínio:** captação e gestão de doações para ONGs — conectar doadores a
campanhas com transparência sobre valores captados.

| Tipo | Escopo | Por quê |
| --- | --- | --- |
| Subdomínio Principal | Gestão de Campanhas, Processamento de Doações, Painel de Transparência | É onde está a regra de negócio real e a complexidade técnica |
| Subdomínio Genérico | Autenticação e Autorização (Identidade) | Padrão de mercado, não diferencia a ONG de nenhum outro sistema |
| Subdomínio de Suporte | Cadastro de Doador | CRUD simples que apoia o core |

## Agregados

Campanha e Doação são **dois agregados separados**, mesmo estando no mesmo
subdomínio principal e no mesmo banco físico:

- **Campanha**: Titulo, Descricao, DataInicio, DataFim, MetaFinanceira,
  Status (Ativa/Concluida/Cancelada), ValorArrecadado
- **Doacao**: IdCampanha (referência por ID, nunca composição), IdDoador,
  ValorDoacao, Status (Pendente/Confirmada/Rejeitada)

Por que não juntar num agregado só ("Campanha contém lista de Doações"): uma
campanha pode ter milhares de doações — modelar como lista interna travaria o
agregado inteiro a cada escrita. "Campanha tem uma lista de doações" é
verdade como **consulta** (FK `IdCampanha` + índice), não como modelagem de
agregado. Além disso, o próprio fluxo assíncrono exigido pelo edital já
pressupõe duas transações distintas — se fossem um agregado só, a atualização
seria uma escrita transacional direta, sem necessidade de fila.

## Arquitetura de serviços

**Modelo: monolito modular com Worker.** Um repositório, dois processos
deployáveis:

| Serviço (pod) | O que contém | Entrada |
| --- | --- | --- |
| `conexao-solidaria-api` | Módulos Identidade + Campanha + Doacao, organizados em pastas (não em projetos/repos separados) | HTTP |
| `doacoes-worker` | Processa `DoacaoRecebidaEvent`, atualiza Campanha e confirma/rejeita a Doação | Fila (RabbitMQ) |

Sem API Gateway (Kong ficou de fora — era opcional no edital).
Autenticação/autorização acontecem dentro da própria API.

```mermaid
%%{init: {"flowchart": {"htmlLabels": true, "curve": "basis", "nodeSpacing": 30, "rankSpacing": 70}, "themeVariables": {"fontFamily": "Helvetica, Arial, sans-serif", "fontSize": "15px"}} }%%
flowchart LR
  subgraph Atores["Atores (HTTP)"]
    direction TB
    Doador
    GestorONG[GestorONG]
    Publico[Publico]
  end

  subgraph API["conexao-solidaria-api (1 processo, 3 módulos)"]
    direction TB
    Identidade[Identidade]
    Campanha[Campanha]
    Doacao[Doacao]
  end

  subgraph DB["Postgres — 1 instância, 3 schemas"]
    direction TB
    SchemaIdentidade[("schema<br/>identidade")]
    SchemaCampanha[("schema<br/>campanha")]
    SchemaDoacao[("schema<br/>doacao")]
  end

  Doador --> API
  GestorONG --> API
  Publico --> API

  Identidade --> SchemaIdentidade
  Campanha --> SchemaCampanha
  Doacao --> SchemaDoacao

  Doacao -->|evento| Fila{{RabbitMQ}}
  Fila --> Worker[["doacoes-worker"]]
  Worker --> SchemaDoacao
  Worker --> SchemaCampanha

  Prometheus((Prometheus)) -.->|scrape| API
  Grafana((Grafana)) -.->|lê| Prometheus

  classDef ator fill:#dbeafe,stroke:#2563eb,stroke-width:1.5px,color:#1e3a8a
  classDef modulo fill:#dcfce7,stroke:#16a34a,stroke-width:1.5px,color:#14532d
  classDef worker fill:#bbf7d0,stroke:#15803d,stroke-width:1.5px,color:#14532d
  classDef fila fill:#ffedd5,stroke:#ea580c,stroke-width:1.5px,color:#7c2d12
  classDef dados fill:#fef3c7,stroke:#d97706,stroke-width:1.5px,color:#78350f
  classDef obs fill:#f3e8ff,stroke:#9333ea,stroke-width:1.5px,color:#581c87

  class Doador,GestorONG,Publico ator
  class Identidade,Campanha,Doacao modulo
  class Worker worker
  class Fila fila
  class SchemaIdentidade,SchemaCampanha,SchemaDoacao dados
  class Prometheus,Grafana obs

  style Atores fill:#eff6ff,stroke:#93c5fd,stroke-width:1px,stroke-dasharray: 4 3,rx:10,ry:10
  style API fill:#f0fdf4,stroke:#86efac,stroke-width:1px,stroke-dasharray: 4 3,rx:10,ry:10
  style DB fill:#fffbeb,stroke:#fde68a,stroke-width:1px,stroke-dasharray: 4 3,rx:10,ry:10
```

Grupos visuais (cor por camada): azul = atores (client-side, batem na API via
HTTP), verde-claro = os três módulos internos da `conexao-solidaria-api`
(Identidade, Campanha, Doacao — pastas dentro do mesmo processo, não serviços
separados), verde-escuro = `doacoes-worker` (o segundo e único outro processo
deployável), laranja = mensageria (RabbitMQ, em hexágono — forma distinta
para destacar a fila), amarelo/dourado = dados (os três schemas do Postgres,
cada um em seu próprio cilindro), roxo = observabilidade (Prometheus e
Grafana, com seta tracejada porque é scrape/leitura, não fluxo de negócio).
Os três atores ficam agrupados num subgraph "Atores" e os três módulos da API
num subgraph "conexao-solidaria-api" só para deixar visualmente explícito que
são, respectivamente, a mesma categoria de cliente e o mesmo processo
deployável — sem implicar nenhuma fronteira de rede entre eles. As três
setas dos atores miram na caixa da `conexao-solidaria-api` como um todo, não
em módulos específicos — este é um diagrama de arquitetura, não de fluxo de
negócio: não importa aqui *qual* ator aciona qual módulo (isso está
detalhado nos `d*.md` de cada demanda), só que os atores acessam a API via
HTTP.

Reparem que só o módulo **Doacao** publica evento na fila — Identidade e
Campanha acessam o próprio schema direto, sem passar pelo RabbitMQ. E o
`doacoes-worker`, mesmo sendo outro processo, ainda acessa os schemas
`doacao` e `campanha` diretamente (não chama a API de volta por HTTP) — por
isso as setas dele miram nos cilindros, não na caixa da API. O diagrama não
detalha *o que* o Worker faz em cada schema (confirma a doação, soma o valor
arrecadado) — isso é regra de negócio, explicada em texto na seção "Banco de
dados" abaixo, não escrita em cima da seta.

## Banco de dados

**1 instância Postgres, 3 schemas** — `identidade`, `campanha`, `doacao` —
não 3 databases separados. Essa escolha é o que permite ao Worker aplicar
uma única transação Postgres real cobrindo os dois schemas ao processar uma
doação:

```sql
begin;
  update campanha.campanhas
    set valor_arrecadado = valor_arrecadado + :valor
    where id = :idCampanha
      and status = 'Ativa'
      and data_fim >= now(); -- campanha vencida não recebe doação (mesmo filtro do painel)
  update doacao.doacoes
    set status = 'Confirmada'
    where id = :idDoacao;
  insert into doacao.doacoes_processadas (id_doacao) values (:idDoacao); -- idempotência
commit;
```

Sem isso (bancos fisicamente separados), seria necessário outbox pattern ou
saga — complexidade que o grupo decidiu evitar dado o prazo.

O padrão de schema-por-módulo **já existe** no repositório base
(`IdentidadeDbContext.SCHEMA = "identidade"`) — só replicar para os módulos
novos.

## Por que a fila continua sendo necessária (mesmo com banco único)

A fila não existe para "ligar dois bancos diferentes" — existe para
desacoplar o *momento* da requisição HTTP do *momento* do processamento:

- **Exigência literal do edital**: broker obrigatório para esse fluxo, e a
  API não pode atualizar o valor arrecadado direto na mesma requisição
- **Latência/disponibilidade**: a API responde `202 Accepted` na hora; o
  processamento roda no ritmo do Worker
- **Resiliência**: mensagem não processada fica na fila e é reprocessada,
  em vez de perder a doação se o processamento falhar no meio
- **Escala independente**: em pico de doações, escala-se só o Worker

O acesso direto ao banco elimina o *salto de rede entre serviços* (não
precisa HTTP de um serviço pro outro); a fila continua sendo o que preserva
a fronteira entre os dois agregados mesmo com banco compartilhado.

## Idempotência

O Worker precisa ser idempotente — se o broker reentregar uma mensagem já
processada, não pode somar o valor duas vezes. Resolvido com a tabela de
controle `doacao.doacoes_processadas` (ver transação acima): antes de
aplicar o incremento, verifica se o `IdDoacao` já foi processado.

## Autenticação e autorização

- JWT **HMAC simétrico** (não RS256 — o repositório base já implementa
  assim, em `JwtService.cs`), emitido e validado pela própria API
- Roles: `GestorONG` e `Doador` (renomeadas a partir do enum `PerfilUsuario`
  existente: `Administrador` → `GestorONG`, `Usuario` → `Doador`)
- Sem Gateway: toda validação de token e checagem de role acontece dentro da
  API, via `[Authorize(Roles = "...")]` padrão do ASP.NET

## Rotas públicas (sem JWT)

- `POST /doadores` — cadastro de doador
- `POST /auth/login`
- `GET /campanhas` — painel de transparência (só campanhas `Status: Ativa`,
  retornando Titulo, MetaFinanceira, ValorArrecadado)

## Estrutura de pastas (Clean Architecture, módulo = pasta)

```
src/
├── ConexaoSolidaria.Domain/{Identidade, Campanhas, Doacao, Shared}/...
├── ConexaoSolidaria.Application/{Identidade, Campanhas, Doacao, Shared}/...
├── ConexaoSolidaria.Infrastructure/{Identidade, Campanhas, Doacao, Shared}/...
├── ConexaoSolidaria.IoC/                         ← composição de dependências
├── ConexaoSolidaria.API/Controllers/{Auth,Conta,Usuario,Campanha,Doacao}Controller.cs
└── ConexaoSolidaria.Worker/                      ← novo projeto, consome DoacaoRecebidaEvent
```

Cada módulo novo (`Campanha`, `Doacao`) segue exatamente a forma de
`Identidade`, que já está implementado no repositório base — ver
`implementacao/fcg-monolito/IMPLEMENTACAO.md` para o mapeamento arquivo a
arquivo.

## Observabilidade e CI/CD (ainda não implementados no repositório base)

- Nenhum Dockerfile existe ainda para a própria API/Worker
- Nenhum pipeline de CI (`.github/workflows`) existe ainda
- Nenhum manifest de Kubernetes existe ainda
- RabbitMQ/MassTransit não estão configurados ainda

Essas quatro coisas são demandas próprias (d2 parcialmente, d3, d4, d5) — não
assumir que algo disso já existe ao ler o código.
