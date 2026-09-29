# Conexão Solidária

MVP da plataforma **Conexão Solidária**, desenvolvido para o Hackathon da
Pós-Graduação em Arquitetura de Sistemas .NET da FIAP (**Turma 11NETT —
Grupo 30**). A plataforma conecta doadores a campanhas de arrecadação de
uma ONG: um `GestorONG` cria e administra campanhas, qualquer pessoa pode
se cadastrar como `Doador` e contribuir, e um painel público mostra o
andamento das campanhas ativas com transparência sobre os valores
arrecadados.

O desenho completo do domínio, dos agregados e das decisões de arquitetura
está em [`docs/conexao-solidaria/ARQUITETURA.md`](docs/conexao-solidaria/ARQUITETURA.md)
— vale a leitura antes de mexer em qualquer parte estrutural do projeto.

## Arquitetura em resumo

Monolito modular em Clean Architecture (.NET 10), com **dois processos
deployáveis** e infraestrutura compartilhada:

| Componente | Papel |
| --- | --- |
| `conexao-solidaria-api` | API HTTP com os módulos Identidade, Campanha e Doação (organizados em pastas, não em repositórios/serviços separados) |
| `doacoes-worker` | Worker que consome `DoacaoRecebidaEvent` da fila, atualiza `ValorArrecadado` da campanha e confirma/rejeita a doação |
| PostgreSQL | 1 instância, **3 schemas** (`identidade`, `campanha`, `doacao`) — não 3 bancos separados |
| RabbitMQ | Broker entre a API (publisher) e o `doacoes-worker` (consumer), via MassTransit |
| Prometheus + Grafana | Scrape do `/metrics` da API e dashboard com métricas reais (requisições, latência, CPU/memória, GC) |

A API nunca atualiza o valor arrecadado de uma campanha na mesma
requisição da doação: ela publica um evento na fila e responde
`202 Accepted`; quem aplica o incremento (de forma atômica e idempotente)
é o `doacoes-worker`. Veja o porquê dessa decisão e o diagrama completo em
[`docs/conexao-solidaria/ARQUITETURA.md`](docs/conexao-solidaria/ARQUITETURA.md).

## Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose (plugin `docker compose`, já incluso no Docker Desktop)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — só necessário para rodar os testes ou a API fora de container
- `bash` (para o smoke test em `e2e/`) — em Windows, use WSL
- Portas livres na máquina: `5432` (Postgres), `5672`/`15672` (RabbitMQ), `8080` (API) e `5050` (PgAdmin)

## Subindo tudo localmente com docker-compose

### 1. Configurar o `.env`

```bash
cp .env.example .env
```

Edite o `.env` gerado e preencha pelo menos:

- `POSTGRES_PASSWORD`, `PGADMIN_DEFAULT_PASSWORD`, `RABBITMQ_PASSWORD` — qualquer senha de sua escolha
- `JWT_SECRET` — gere uma com `openssl rand -base64 32` (mínimo de 32 caracteres, ou a API falha ao subir)

Os demais valores (`POSTGRES_USER`, `POSTGRES_DB`, `PGADMIN_DEFAULT_EMAIL`,
`RABBITMQ_USER`) já vêm preenchidos no `.env.example` e podem ser mantidos
como estão.

> Existe um segundo `.env.example` em `src/ConexaoSolidaria.API/`, usado só
> se você quiser rodar a API diretamente com `dotnet run` (fora de
> container) — para subir via docker-compose, o `.env` da raiz é
> suficiente.

### 2. Subir o stack

```bash
docker compose up -d --build
```

Isso builda as imagens e sobe, nesta ordem (o `docker-compose.yml` já
expressa as dependências com `depends_on`/healthcheck):

1. `postgres` (Postgres 18) e `rabbitmq` (RabbitMQ 4.1 com plugin de management)
2. `migrate` — container efêmero que aplica as migrations do EF Core nos
   três schemas (`identidade`, `campanha`, `doacao`) e depois termina
3. `api` — sobe só depois do `migrate` terminar com sucesso e do `rabbitmq` ficar saudável
4. `doacoes-worker` — mesmas dependências da `api`

O `pgadmin` não depende de nada além do Postgres e sobe em paralelo.

### 3. Confirmar que subiu certo

```bash
docker compose ps
```

Espera-se `postgres`, `rabbitmq`, `api`, `doacoes-worker` e `pgadmin`
com status `running`/`healthy`, e `migrate` com status `exited (0)`
(ele roda uma vez só e termina — não é um erro ele aparecer parado).

Conferir a API:

```bash
curl http://localhost:8080/health
```

Deve responder `Healthy` (checa Postgres e RabbitMQ). Para acompanhar logs
de qualquer serviço: `docker compose logs -f api` (ou `doacoes-worker`,
`postgres`, etc.).

### Portas expostas

| Serviço | Porta local | Uso |
| --- | --- | --- |
| `api` | `8080` | HTTP da API (Swagger, Scalar, endpoints) |
| `postgres` | `5432` | Conexão direta ao banco (psql, PgAdmin externo, etc.) |
| `rabbitmq` | `5672` | Protocolo AMQP |
| `rabbitmq` | `15672` | Console de management (`http://localhost:15672`) |
| `pgadmin` | `5050` | Console web do PgAdmin (`http://localhost:5050`) |

`doacoes-worker` não expõe porta — é um host que só consome a fila.

### Parar / limpar

```bash
docker compose down       # para os containers, mantém os volumes (dados do Postgres)
docker compose down -v    # para e apaga os volumes (cuidado: apaga os dados do banco)
```

## Credenciais de teste

A migration de seed do módulo Identidade já cria um usuário `GestorONG`:

```json
{
  "email": "admin@conexaosolidaria.com.br",
  "senha": "Admin@123"
}
```

Use-o para autenticar em `POST /api/auth/login` e testar os endpoints
restritos a `GestorONG` (criar/editar campanha, gestão de usuários).

Para testar como **Doador**, registre um usuário novo em
`POST /api/auth/registrar` — todo cadastro público nasce com o perfil
`Doador`:

```json
{
  "nome": "Doador de Teste",
  "email": "doador@teste.com",
  "cpf": "111.444.777-35",
  "senha": "Senha@123",
  "confirmacaoSenha": "Senha@123"
}
```

O CPF é validado de verdade (dígitos verificadores, não só "11 dígitos") e
precisa ser único por doador — ver `docs/conexao-solidaria/plano/d0.1-cpf-doador.md`
para o histórico da implementação.

## Endpoints principais

Com o stack no ar, a documentação interativa da API fica disponível em:

- Swagger UI: `http://localhost:8080/swagger`
- Scalar: `http://localhost:8080/scalar/v1`

### Rotas públicas (sem token)

| Rota | Descrição |
| --- | --- |
| `POST /api/auth/registrar` | Cadastro de doador |
| `POST /api/auth/login` | Login, retorna access token + refresh token |
| `POST /api/auth/refresh` | Rotação de refresh token |
| `GET /api/campanhas` | Painel de transparência público — só campanhas `Ativa`, dentro do período, com `Id`, `Titulo`, `MetaFinanceira` e `ValorArrecadado` |

### Rotas autenticadas (JWT Bearer)

| Rota | Perfil exigido | Descrição |
| --- | --- | --- |
| `GET /api/conta` | qualquer usuário autenticado | Dados da própria conta |
| `PATCH /api/conta/senha` | qualquer usuário autenticado | Troca de senha |
| `POST /api/auth/logout` | qualquer usuário autenticado | Revoga o refresh token |
| `POST /api/campanhas` | `GestorONG` | Cria campanha (nasce com `Status: Ativa`) |
| `PUT /api/campanhas/{id}` | `GestorONG` | Edita campanha (campos opcionais) |
| `POST /api/doacoes` | `Doador` | Registra intenção de doação — responde `202 Accepted` e publica o evento na fila |
| `GET /api/doacoes/{id}` | `Doador` (só o dono) | Consulta uma doação própria |
| `GET /api/doacoes` | `Doador` | Lista as próprias doações |
| `GET /api/doacoes/admin` | `GestorONG` | Lista doações de todos os doadores |
| `GET /api/usuarios`, `GET /api/usuarios/{id}` | `GestorONG` | Gestão de usuários |
| `PATCH /api/usuarios/{id}/tornar-administrador`, `/inativar`, `/ativar` | `GestorONG` | Gestão de usuários |

Todas as datas trafegam em **UTC** (ISO 8601). Uma data enviada sem fuso —
ex.: `2026-12-31T23:59:59` — é interpretada como UTC.

### Fluxos principais, ponta a ponta

1. **Registrar doador** — `POST /api/auth/registrar` com nome, e-mail e senha
2. **Autenticar** — `POST /api/auth/login` (funciona tanto para o admin de
   seed quanto para o doador recém-criado) — guarde o `accessToken`
3. **Criar campanha** (como `GestorONG`, com o token do admin de seed) —
   `POST /api/campanhas` com `Titulo`, `Descricao`, `DataInicio`, `DataFim`
   e `MetaFinanceira`; resposta `201` com `{ "id": "..." }`
4. **Listar campanhas públicas** — `GET /api/campanhas`, sem token, mostra
   a campanha recém-criada (se já iniciada e dentro do prazo)
5. **Fazer uma doação** (como `Doador`) — `POST /api/doacoes` com
   `IdCampanha` e `ValorDoacao`; resposta `202 Accepted`. O valor
   arrecadado só aparece atualizado em `GET /api/campanhas` depois que o
   `doacoes-worker` processar a mensagem na fila `doacao-recebida`
   (visível em `http://localhost:15672`)

> A validação ponta a ponta desse fluxo completo contra Postgres/RabbitMQ
> reais (via `docker compose up`) está registrada como pendência em
> [`docs/conexao-solidaria/plano/d2-doacao-worker.md`](docs/conexao-solidaria/plano/d2-doacao-worker.md)
> — o fluxo foi validado com a API e o Worker subindo isoladamente
> (`dotnet run`), mas não com o broker/banco reais rodando juntos até o
> fechamento desta versão do README. Teste antes de confiar cegamente no
> passo 5 em uma nova máquina.

## Rodando os testes

Testes unitários (xUnit) da solução:

```bash
dotnet test ConexaoSolidaria.slnx
```

Smoke test E2E da API de Identidade (sobe o stack via docker-compose do
zero, roda uma bateria de cenários HTTP contra `http://localhost:8080` e
derruba tudo ao final — não deixa containers/volumes para trás):

```bash
bash e2e/smoke-test.sh
```

Pré-requisitos do smoke test: Docker (com `docker compose`), `jq`,
`python3`, `nc`, `openssl`, e as portas `8080`/`5432` livres. Detalhes do
que é coberto em [`e2e/README.md`](e2e/README.md). Convenção do projeto:
rodar esse script depois de qualquer mudança na API de Identidade, antes
de abrir PR.

## CI/CD

O workflow [`.github/workflows/docker-publish.yml`](.github/workflows/docker-publish.yml)
roda a cada push na `main` (e também pode ser disparado manualmente):

1. **`test`** — restaura dependências e roda `dotnet test` da solução
2. **`build-and-push`** (depende do `test` passar) — builda as imagens
   `conexao-solidaria-api` e `doacoes-worker` a partir dos respectivos
   Dockerfiles e publica ambas no GitHub Container Registry
   (`ghcr.io/11nettg30/conexao-solidaria-api` e
   `ghcr.io/11nettg30/doacoes-worker`, tag `latest`), removendo em seguida
   versões antigas sem tag

## Deploy em Kubernetes

Os manifests estão em [`k8s/`](k8s/), com um `README.md` próprio
([`k8s/README.md`](k8s/README.md)) cobrindo o passo a passo completo. Resumo:

```bash
# cluster local (kind, como exemplo)
kind create cluster --name conexao-solidaria

# copiar os secrets de exemplo para os nomes reais e preencher com valores próprios
cp k8s/conexao-solidaria-api/secret.example.yaml k8s/conexao-solidaria-api/secret.yaml
cp k8s/doacoes-worker/secret.example.yaml k8s/doacoes-worker/secret.yaml
cp k8s/postgres/secret.example.yaml k8s/postgres/secret.yaml
cp k8s/rabbitmq/secret.example.yaml k8s/rabbitmq/secret.yaml
cp k8s/grafana/secret.example.yaml k8s/grafana/secret.yaml

# build das imagens locais e carga no cluster (se não for usar as imagens do GHCR)
docker build -t conexao-solidaria-api:local -f src/ConexaoSolidaria.API/Dockerfile .
docker build -t doacoes-worker:local -f src/ConexaoSolidaria.Worker/Dockerfile .
kind load docker-image conexao-solidaria-api:local --name conexao-solidaria
kind load docker-image doacoes-worker:local --name conexao-solidaria

# aplicar os manifests (ordem completa e detalhada em k8s/README.md)
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/postgres/ -f k8s/rabbitmq/
kubectl apply -f k8s/conexao-solidaria-api/ -f k8s/doacoes-worker/
kubectl apply -f k8s/prometheus/ -f k8s/grafana/
kubectl apply -f k8s/ingress.yaml
```

Verificação:

```bash
kubectl get pods -n conexao-solidaria
```

O cluster sobe também **Prometheus** (scrape do `/metrics` da API) e
**Grafana**, já provisionado com o dashboard "Conexão Solidária — API"
(requisições por rota/status, latência p50/p95/p99, CPU/memória por pod,
GC, threads). Acesso ao Grafana:

```bash
kubectl port-forward svc/grafana 3000:3000 -n conexao-solidaria
# ou, se o Ingress estiver configurado no /etc/hosts local:
# http://grafana.conexaosolidaria.local
```

> A validação de ponta a ponta do `kubectl apply -f k8s/` completo está
> marcada como pendente no checklist da demanda
> [`docs/conexao-solidaria/plano/d4-kubernetes.md`](docs/conexao-solidaria/plano/d4-kubernetes.md),
> embora a d5 (observabilidade) relate ter rodado o conjunto todo com
> sucesso num cluster Kubernetes do Docker Desktop em 2026-09-27.

## Estrutura do repositório

```
src/
├── ConexaoSolidaria.API/            ← Controllers, Program.cs, Dockerfile (API + estágio migrate)
├── ConexaoSolidaria.Application/     ← DTOs, UseCases, Validators — por módulo (Identidade, Campanhas, Doacoes)
├── ConexaoSolidaria.Domain/          ← Entidades, Value Objects, interfaces de repositório
├── ConexaoSolidaria.Infrastructure/  ← EF Core, JWT, Argon2id, MassTransit, OpenTelemetry
├── ConexaoSolidaria.IoC/              ← composição de dependências
└── ConexaoSolidaria.Worker/          ← doacoes-worker: consome DoacaoRecebidaEvent
tests/
└── ConexaoSolidaria.Tests/           ← testes unitários (xUnit), por módulo
docs/conexao-solidaria/              ← arquitetura, contexto técnico e planos de cada demanda
e2e/                                  ← smoke test E2E via docker-compose
k8s/                                   ← manifests Kubernetes (API, Worker, Postgres, RabbitMQ, Prometheus, Grafana)
edital/                                ← edital completo do hackathon
```

Mais detalhes de stack e do padrão de módulo em
[`docs/conexao-solidaria/CONTEXTO-TECNICO.md`](docs/conexao-solidaria/CONTEXTO-TECNICO.md).
