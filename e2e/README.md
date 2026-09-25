# Teste E2E (smoke test) — Identidade

`smoke-test.sh` sobe o stack completo via `docker-compose` (build das
imagens `migrate`/`api`, migrations do EF Core contra um Postgres limpo,
API no ar) e roda uma bateria de cenários HTTP contra `http://localhost:8080`,
conferindo tanto a resposta da API quanto o estado gravado no banco.

## O que é coberto

- Registro de doador, e-mail duplicado (409), senha fraca sem maiúscula
  (400), confirmação de senha divergente (400), e-mail inválido (400)
- Login (sucesso, senha errada, usuário inativo) e claim `role` do JWT
- `GET /api/conta` autenticado/não autenticado, troca de senha
- Refresh token (rotação, marcação do token antigo como substituído),
  logout (revogação), refresh com token inexistente
- Autorização por perfil: `GET/PATCH /api/usuarios/*` negado para `Doador`
  (403), permitido para `GestorONG`
- `tornar-administrador`, `inativar` (revoga refresh tokens ativos com
  `motivo_revogacao = 4`) e `ativar` um usuário
- Senha nunca armazenada em texto puro (hash Argon2id no banco)

Ao final o script derruba o stack (`docker compose down -v`), então não
deixa containers nem volumes para trás.

## Como rodar

Pré-requisitos: Docker (com `docker compose`), `jq`, `python3`, `nc`,
`openssl`. Portas `8080` e `5432` precisam estar livres.

```bash
bash e2e/smoke-test.sh
```

Se não existir um `.env` na raiz do repositório, o script gera um
temporário (credenciais aleatórias + `JWT_SECRET`) só para essa execução e
o remove ao final — não mexe num `.env` de desenvolvimento que já exista.

O script termina com `PASS=N FAIL=0` e código de saída `0` quando tudo
passa; qualquer `FAIL` faz o script sair com código diferente de zero.

Rodando atrás de um proxy de build que reassina TLS (ex.: sandbox de
execução remota do Claude Code), o script detecta o CA bundle local do
proxy e injeta em `docker/certs/` antes do `docker compose build`, para o
`dotnet restore` dentro do container confiar nele — sem nenhuma ação
manual. Ver `docker/certs/README.md`.

## Quando rodar

Depois de mudanças na API de Identidade (controllers, use cases,
middlewares de exceção, JWT, migrations) e antes de abrir um PR. Não
precisa rodar a cada commit — é uma bateria pesada (builda as imagens do
zero) pensada para validar o fluxo ponta a ponta, não para uso no dia a
dia de desenvolvimento.
