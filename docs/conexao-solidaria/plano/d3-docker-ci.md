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

- `fcg-api` (a partir de `src/FCG.API`)
- `doacoes-worker` (a partir de `src/FCG.Worker`)

## Andamento

- [ ] `Dockerfile` multi-stage para `FCG.API`
- [ ] `Dockerfile` multi-stage para `FCG.Worker`
- [ ] Adicionar os dois serviços ao `docker-compose.yml` (junto com
      Postgres, PgAdmin e RabbitMQ já existentes/adicionados em D2), para
      teste local de ponta a ponta
- [ ] Workflow `.github/workflows/docker-build.yml`: build + push das duas
      imagens para GitHub Container Registry a cada push na branch principal
- [ ] Confirmar que o pipeline roda os testes (`dotnet test`) antes do build
      da imagem

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (seção de requisitos
  técnicos obrigatórios)
