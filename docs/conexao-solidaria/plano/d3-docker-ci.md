# D3 — Dockerfile + CI/CD

Responsável: quem estiver livre primeiro entre D1/D2. Depende de a API e o
Worker compilarem (não precisa das regras de negócio 100% prontas, só
compilando).

## Objetivo

Containerizar a API e o Worker, e ter um pipeline que builda a imagem a cada
push — exigência obrigatória do edital.

## Instruções

Herda tudo de `visao-geral.md`. Ler `../CONTEXTO-TECNICO.md`
para saber que **nenhum** Dockerfile ou workflow existe ainda no repositório
— não presumir que há algo para "ajustar", é criação do zero.

## Contexto

Exigência do edital: "O repositório deve conter um pipeline (GitHub Actions,
Azure DevOps, etc.) que seja acionado a cada push na branch principal. O
pipeline deve compilar o código (.NET build) e gerar a imagem Docker. O
deploy automatizado no Kubernetes é opcional, mas a geração da imagem no CI
é obrigatória."

Dois processos deployáveis (ver `../ARQUITETURA.md`) → duas
imagens:

- `conexao-solidaria-api` (a partir de `src/ConexaoSolidaria.API`)
- `doacoes-worker` (a partir de `src/ConexaoSolidaria.Worker`)

## Andamento

- [x] `Dockerfile` multi-stage para `ConexaoSolidaria.API` (raiz do repo) — estágios
      `build` → `migrate` (aplica `dotnet ef database update` antes da API
      subir) → `final` (runtime `aspnet`, porta 8080)
- [ ] `Dockerfile` multi-stage para `ConexaoSolidaria.Worker` — bloqueado: o projeto
      `ConexaoSolidaria.Worker` ainda não existe (depende de D2)
- [x] Adicionar o serviço `api` (+ `migrate`) ao `docker-compose.yml`, junto
      com o Postgres já existente, para teste local de ponta a ponta —
      RabbitMQ/`doacoes-worker` ficam para quando D2 entrar
- [ ] Workflow `.github/workflows/docker-build.yml`: build + push das duas
      imagens para GitHub Container Registry a cada push na branch principal
- [ ] Confirmar que o pipeline roda os testes (`dotnet test`) antes do build
      da imagem

## Notas de implementação

- Validado localmente (build + `docker compose up`: postgres → migrate →
  api) em 2026-09-24: cadastro, login e rota protegida (`GET /api/conta`)
  respondendo corretamente, com a claim de role já saindo `Doador`/`GestorONG`
  (confirma o rename da d0 ponta a ponta).
- `.config/dotnet-tools.json` criado (`dotnet-ef` como tool local) — o
  estágio `migrate` do Dockerfile depende dele para aplicar migrations sem
  exigir o SDK do EF instalado globalmente no host de deploy.
- Achado durante o teste: `dotnet ef database update` builda em `Debug` por
  padrão; como só publicamos artefatos em `Release`, isso quebrava a
  resolução de recursos (`MSB3552`). Corrigido passando
  `--configuration Release` no `ENTRYPOINT` do estágio `migrate`.
- Connection string e `JwtSettings:Secret` são injetados via variáveis de
  ambiente no `docker-compose.yml` (não dependem dos valores hardcoded em
  `appsettings.json`) — ver `.env.example` (`JWT_SECRET`).
- `ASPNETCORE_ENVIRONMENT=Development` por padrão no serviço `api` só para
  manter Swagger/Scalar acessíveis nesse estágio do projeto; revisar antes
  de qualquer deploy que se pretenda "de produção" de verdade.

## Pendências / dúvidas

Nenhuma no momento. (O mismatch `PGADMIN_EMAIL`/`PGADMIN_PASSWORD` vs
`PGADMIN_DEFAULT_EMAIL`/`PGADMIN_DEFAULT_PASSWORD` no `docker-compose.yml`,
registrado aqui antes, foi corrigido — `pgadmin` agora lê as mesmas
variáveis documentadas em `.env.example`. Achado de forma independente
também durante o E2E aprofundado no Render, o que confirmou o mismatch.)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (seção de requisitos
  técnicos obrigatórios)
