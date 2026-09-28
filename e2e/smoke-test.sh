#!/bin/bash
# Bateria de teste E2E do MVP Conexão Solidária: sobe o stack via docker-compose,
# exercita a API de Identidade por HTTP e confere o estado direto no Postgres.
# Ver e2e/README.md para instruções de uso.
set -uo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

BASE="http://localhost:8080"
PASS=0
FAIL=0
declare -a RESULTS
EMAIL="doador.e2e.$(date +%s)@teste.com"
ENV_CRIADO=0
CERTS_DIR="$REPO_ROOT/docker/certs"
CA_SANDBOX="/root/.ccr/ca-bundle.crt"
CA_COPIADO=0

check() {
  local name="$1" expected="$2" actual="$3"
  actual="$(echo -n "$actual" | tr -d '[:space:]')"
  if [ "$expected" = "$actual" ]; then
    RESULTS+=("PASS | $name | esperado=$expected obtido=$actual")
    PASS=$((PASS+1))
  else
    RESULTS+=("FAIL | $name | esperado=$expected obtido=$actual")
    FAIL=$((FAIL+1))
  fi
}
note() { RESULTS+=("NOTE | $1"); }

db() {
  docker compose exec -T -e Q="$1" postgres bash -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -t -A -c "$Q"' 2>/dev/null
}

porta_em_uso() {
  local porta="$1"
  nc -z localhost "$porta" >/dev/null 2>&1
}

cleanup() {
  echo "== Limpando =="
  docker compose down -v >/dev/null 2>&1
  if [ "$ENV_CRIADO" = "1" ]; then
    rm -f "$REPO_ROOT/.env"
    note ".env temporário (gerado por este script) removido ao final"
  fi
  if [ "$CA_COPIADO" = "1" ]; then
    rm -f "$CERTS_DIR/sandbox-proxy.crt"
    note "CA extra do sandbox removida de docker/certs/ ao final"
  fi
}
trap cleanup EXIT

for dep in docker jq python3 nc; do
  if ! command -v "$dep" >/dev/null 2>&1; then
    echo "Dependência ausente: $dep. Veja e2e/README.md para os pré-requisitos." >&2
    exit 1
  fi
done

for porta in 8080 5432; do
  if porta_em_uso "$porta"; then
    echo "Porta $porta já está em uso. Libere-a (ou pare o processo/stack que a está usando) antes de rodar este script." >&2
    exit 1
  fi
done

if [ ! -f "$REPO_ROOT/.env" ]; then
  echo "== .env não encontrado: gerando um temporário para o teste =="
  cat > "$REPO_ROOT/.env" <<EOF
POSTGRES_USER=conexaosolidaria
POSTGRES_PASSWORD=e2e_$(openssl rand -hex 8)
POSTGRES_DB=conexao_solidaria
PGADMIN_DEFAULT_EMAIL=admin@conexaosolidaria.com.br
PGADMIN_DEFAULT_PASSWORD=e2e_$(openssl rand -hex 8)
JWT_SECRET=$(openssl rand -base64 32)
ASPNETCORE_ENVIRONMENT=Development
EOF
  ENV_CRIADO=1
  note ".env temporário gerado para esta execução (não afeta seu .env de desenvolvimento, que não existia)"
fi

echo "== Subindo stack =="
docker compose down -v >/dev/null 2>&1 || true
docker compose up -d postgres
for i in $(seq 1 30); do
  status=$(docker inspect --format='{{.State.Health.Status}}' conexao-solidaria-postgres 2>/dev/null)
  [ "$status" = "healthy" ] && break
  sleep 2
done
if [ "$status" != "healthy" ]; then
  echo "Postgres não ficou healthy a tempo. Abortando." >&2
  exit 1
fi
if [ -f "$CA_SANDBOX" ]; then
  cp "$CA_SANDBOX" "$CERTS_DIR/sandbox-proxy.crt"
  CA_COPIADO=1
  note "CA do proxy de egress do sandbox detectada e injetada em docker/certs/ para o build (ver docker/certs/README.md)"
fi

echo "== Compilando imagens migrate/api =="
if ! docker compose build migrate api; then
  echo "Build das imagens migrate/api falhou. Veja o log do docker compose build acima." >&2
  exit 1
fi
docker compose up migrate
docker compose up -d api
sleep 4

echo "== A. Registrar doador1 ($EMAIL) =="
CODE=$(curl -s -o /tmp/r_a.json -w "%{http_code}" -X POST "$BASE/api/auth/registrar" -H "Content-Type: application/json" \
  -d "{\"nome\":\"Doadora E2E\",\"email\":\"$EMAIL\",\"cpf\":\"104.332.181-00\",\"senha\":\"Teste@123\",\"confirmacaoSenha\":\"Teste@123\"}")
check "A registrar doador1" "201" "$CODE"

DOADOR1_ID=$(db "select id from identidade.usuarios where email='$EMAIL';")
DOADOR1_PERFIL=$(db "select perfil from identidade.usuarios where email='$EMAIL';")
DOADOR1_ATIVO=$(db "select ativo from identidade.usuarios where email='$EMAIL';")
check "A DB: perfil=1 (Doador)" "1" "$DOADOR1_PERFIL"
check "A DB: ativo=t" "t" "$DOADOR1_ATIVO"

echo "== B. Registrar mesmo email de novo (deve rejeitar) =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/registrar" -H "Content-Type: application/json" \
  -d "{\"nome\":\"Doadora E2E\",\"email\":\"$EMAIL\",\"cpf\":\"104.332.181-00\",\"senha\":\"Teste@123\",\"confirmacaoSenha\":\"Teste@123\"}")
check "B email duplicado" "409" "$CODE"

echo "== C. Login com senha errada =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"senha\":\"SenhaErrada123\"}")
check "C login senha errada" "401" "$CODE"

echo "== D. Login correto =="
CODE=$(curl -s -o /tmp/r_d.json -w "%{http_code}" -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"senha\":\"Teste@123\"}")
check "D login correto" "200" "$CODE"
DOADOR1_ACCESS=$(jq -r '.accessToken' /tmp/r_d.json)
DOADOR1_REFRESH=$(jq -r '.refreshToken' /tmp/r_d.json)
ROLE_CLAIM=$(echo "$DOADOR1_ACCESS" | cut -d'.' -f2 | tr '_-' '/+' | python3 -c "import sys,base64,json; s=sys.stdin.read().strip(); s+='='*(-len(s)%4); print(json.loads(base64.b64decode(s))['role'])" 2>&1)
check "D claim role=Doador" "Doador" "$ROLE_CLAIM"
REFRESH_COUNT=$(db "select count(*) from identidade.refresh_tokens where usuario_id='$DOADOR1_ID';")
check "D DB: 1 refresh_token criado" "1" "$REFRESH_COUNT"

echo "== E. GET /api/conta sem token =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/conta")
check "E conta sem token" "401" "$CODE"

echo "== F. GET /api/conta com token =="
CODE=$(curl -s -o /tmp/r_f.json -w "%{http_code}" "$BASE/api/conta" -H "Authorization: Bearer $DOADOR1_ACCESS")
check "F conta com token" "200" "$CODE"
check "F email bate" "$EMAIL" "$(jq -r '.email' /tmp/r_f.json)"

echo "== G. Trocar senha =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$BASE/api/conta/senha" -H "Authorization: Bearer $DOADOR1_ACCESS" -H "Content-Type: application/json" \
  -d '{"senhaAtual":"Teste@123","novaSenha":"NovaSenha@456","confirmacaoNovaSenha":"NovaSenha@456"}')
check "G trocar senha" "204" "$CODE"
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"senha\":\"NovaSenha@456\"}")
check "G login com senha nova funciona" "200" "$CODE"

echo "== H. Refresh token =="
CODE=$(curl -s -o /tmp/r_h.json -w "%{http_code}" -X POST "$BASE/api/auth/refresh" -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$DOADOR1_REFRESH\"}")
check "H refresh" "200" "$CODE"
NOVO_REFRESH=$(jq -r '.refreshToken' /tmp/r_h.json)
check "H DB: refresh antigo marcado como substituído" "t" "$(db "select substituido_por_id is not null from identidade.refresh_tokens where token='$DOADOR1_REFRESH';")"

echo "== I. Logout =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/logout" -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$NOVO_REFRESH\"}")
check "I logout" "204" "$CODE"
check "I DB: refresh_token revogado=t após logout" "t" "$(db "select revogado from identidade.refresh_tokens where token='$NOVO_REFRESH';")"

echo "== J. Login como admin seed =="
CODE=$(curl -s -o /tmp/r_j.json -w "%{http_code}" -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" \
  -d '{"email":"admin@conexaosolidaria.com.br","senha":"Admin@123"}')
check "J login admin seed" "200" "$CODE"
ADMIN_ACCESS=$(jq -r '.accessToken' /tmp/r_j.json)

echo "== K. GET /api/usuarios com token de doador (deve negar) =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/usuarios" -H "Authorization: Bearer $DOADOR1_ACCESS")
check "K usuarios com token doador" "403" "$CODE"

echo "== L. GET /api/usuarios com token admin =="
CODE=$(curl -s -o /tmp/r_l.json -w "%{http_code}" "$BASE/api/usuarios" -H "Authorization: Bearer $ADMIN_ACCESS")
check "L usuarios com token admin" "200" "$CODE"
check "L doador1 aparece na listagem" "1" "$(jq -r --arg id "$DOADOR1_ID" '[.[] | select(.id==$id)] | length' /tmp/r_l.json)"

echo "== M. GET /api/usuarios/{id} com token admin =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/usuarios/$DOADOR1_ID" -H "Authorization: Bearer $ADMIN_ACCESS")
check "M usuarios/{id}" "200" "$CODE"

echo "== N. Tornar doador1 GestorONG =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$BASE/api/usuarios/$DOADOR1_ID/tornar-administrador" -H "Authorization: Bearer $ADMIN_ACCESS")
check "N tornar-administrador" "204" "$CODE"
check "N DB: perfil agora = 2 (GestorONG)" "2" "$(db "select perfil from identidade.usuarios where id='$DOADOR1_ID';")"

echo "== O. Inativar doador1 (deve revogar refresh tokens ativos) =="
LOGIN2=$(curl -s -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" -d "{\"email\":\"$EMAIL\",\"senha\":\"NovaSenha@456\"}")
REFRESH_ANTES_INATIVAR=$(echo "$LOGIN2" | jq -r '.refreshToken')
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$BASE/api/usuarios/$DOADOR1_ID/inativar" -H "Authorization: Bearer $ADMIN_ACCESS")
check "O inativar" "204" "$CODE"
check "O DB: ativo=f" "f" "$(db "select ativo from identidade.usuarios where id='$DOADOR1_ID';")"
check "O DB: refresh_token revogado ao inativar" "t" "$(db "select revogado from identidade.refresh_tokens where token='$REFRESH_ANTES_INATIVAR';")"
check "O DB: motivo_revogacao=4 (InativacaoUsuario)" "4" "$(db "select motivo_revogacao from identidade.refresh_tokens where token='$REFRESH_ANTES_INATIVAR';")"

echo "== P. Login com usuário inativo deve falhar =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"senha\":\"NovaSenha@456\"}")
check "P login usuario inativo" "401" "$CODE"

echo "== Q. Reativar doador1 =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$BASE/api/usuarios/$DOADOR1_ID/ativar" -H "Authorization: Bearer $ADMIN_ACCESS")
check "Q ativar" "204" "$CODE"
check "Q DB: ativo=t novamente" "t" "$(db "select ativo from identidade.usuarios where id='$DOADOR1_ID';")"

echo "== R. Registrar com senha fraca (sem maiúscula) =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/registrar" -H "Content-Type: application/json" \
  -d '{"nome":"X","email":"senhafraca.e2e@teste.com","cpf":"960.013.389-14","senha":"minuscula123!","confirmacaoSenha":"minuscula123!"}')
check "R senha sem maiuscula rejeitada" "400" "$CODE"

echo "== S. Registrar com confirmação de senha diferente =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/registrar" -H "Content-Type: application/json" \
  -d '{"nome":"X","email":"confdiff.e2e@teste.com","cpf":"083.863.794-99","senha":"Teste@123","confirmacaoSenha":"Outra@123"}')
check "S confirmacao diferente rejeitada" "400" "$CODE"

echo "== T. Registrar com email inválido =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/registrar" -H "Content-Type: application/json" \
  -d '{"nome":"X","email":"nao-e-email","cpf":"026.542.351-14","senha":"Teste@123","confirmacaoSenha":"Teste@123"}')
check "T email invalido rejeitado" "400" "$CODE"

echo "== U. Refresh com token inexistente =="
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/refresh" -H "Content-Type: application/json" \
  -d '{"refreshToken":"11111111-1111-1111-1111-111111111111"}')
check "U refresh token inexistente" "401" "$CODE"

echo "== V. Senha nunca fica em texto puro no banco =="
SENHA_DB=$(db "select senha from identidade.usuarios where email='$EMAIL';")
if [[ "$SENHA_DB" != *"NovaSenha@456"* ]] && [[ "$SENHA_DB" == *"."* ]] && [ "${#SENHA_DB}" -ge 60 ]; then
  check "V senha armazenada como hash" "ok" "ok"
else
  check "V senha armazenada como hash" "ok" "FALHOU: $SENHA_DB"
fi

echo ""
echo "=================== RESULTADO ==================="
for r in "${RESULTS[@]}"; do echo "$r"; done
echo "===================================================="
echo "PASS=$PASS FAIL=$FAIL"

[ "$FAIL" -eq 0 ]
