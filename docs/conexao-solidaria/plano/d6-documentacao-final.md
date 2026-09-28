# D6 — Documentação final e vídeo

Depende de tudo (d0–d5). É a última demanda antes da entrega. Detalhado
abaixo em blocos, porque **nem tudo aqui depende de d2–d5 estarem prontas**
— dá pra dividir entre o grupo e começar boa parte em paralelo.

## Objetivo

Fechar os entregáveis mínimos do edital que não são código.

## Instruções

Herda tudo de `visao-geral.md`.

## Contexto

Exigências do edital:

1. **README.md** do repositório com passo a passo claro de como subir
   infraestrutura + aplicação localmente
2. **Diagrama de arquitetura** claro (microsserviços, bancos, broker,
   observabilidade) — `../ARQUITETURA.md` já tem o diagrama
   mermaid, só precisa exportar/adaptar para o formato de entrega
3. **Documento PDF** justificando a escolha dos bancos de dados
4. **Vídeo de demonstração** (máx. 15 min), roteiro obrigatório:
   - Explicação do diagrama de arquitetura
   - Pipeline de CI executando e gerando a imagem Docker
   - Terminal com `kubectl get pods` + dashboard Grafana com dados reais
   - Autenticação via Postman/Swagger + token JWT
   - Criação de uma campanha
   - Simulação de uma doação: payload enviado → RabbitMQ mostrando a
     mensagem → API pública confirmando que o valor foi atualizado
5. **Relatório de entrega** (PDF ou TXT): nome do grupo, participantes +
   usernames Discord, link da documentação, link do(s) repositório(s), link
   do vídeo

## Divisão por blocos (o que depende de quê)

### Bloco A — pode começar agora, não depende de d2/d3/d4/d5

- Diagrama de arquitetura exportado (item 2) — o mermaid já existe em
  `../ARQUITETURA.md`, é só virar imagem/anexo no formato de entrega
- PDF de justificativa de banco (item 3) — o conteúdo técnico já está
  escrito em `../ARQUITETURA.md`, seção "Banco de dados" (1 Postgres, 3
  schemas, por que não bancos separados); é reescrever em prosa e exportar
- Campos fixos do relatório de entrega (item 5, parcial): nome do grupo,
  participantes, usernames no Discord, link do repositório — nenhum depende
  de código rodando
- Esqueleto do README (item 1, parcial): pré-requisitos, estrutura de
  pastas, seção de Identidade + Campanha (d0/d1, já funcionando e validado)

### Bloco B — depende só de d2 fechada (não de d3/d4/d5)

- Seção do README sobre RabbitMQ/Worker/fluxo de doação via
  `docker-compose` (item 1)
- Roteiro de texto (não a gravação) dos trechos "Autenticação via
  Postman/Swagger", "Criação de campanha" e "Simulação de doação" do vídeo
  (item 4, sub-itens d/e/f do roteiro) — dá pra escrever o texto/passo a
  passo antes de gravar, mas a gravação de verdade da doação só depois que
  a validação ponta a ponta da d2 existir (ver `d2-doacao-worker.md`)

### Bloco C — depende de d3 (pipeline CI) — **já destravado, d3 está pronta**

- Seção do README sobre o pipeline, se fizer sentido documentar
- Gravação do trecho "Pipeline de CI executando e gerando a imagem Docker"
  do vídeo (item 4)

### Bloco D — depende de d4 + d5 (Kubernetes + observabilidade) — **d5 está
pronta e validada; d4 tem os manifests prontos, só falta confirmar
formalmente o `kubectl apply -f k8s/` ponta a ponta (ver `d4-kubernetes.md`)
— na prática pode já estar coberto, já que a d5 relata ter validado tudo
rodando junto num cluster Docker Desktop**

- Seção do README sobre deploy em Kubernetes e acesso ao Grafana
- Gravação do trecho "Terminal com `kubectl get pods` + dashboard Grafana
  com dados reais" do vídeo (item 4)

### Bloco E — final, depende de TUDO (a, b, c, d fechados)

- Gravação e montagem final do vídeo completo (máx. 15 min), juntando os
  trechos gravados nos blocos B/C/D com a explicação do diagrama (bloco A)
- Fechar o relatório de entrega (item 5) com o link do vídeo publicado

**Ordem prática** (atualizado em 2026-09-28, depois do PR #17 mergear d3+d5):
Bloco A pode ser feito por qualquer pessoa a qualquer momento. **Blocos C e
D já estão destravados** — d3 está pronta e d5 está pronta e validada num
cluster real (d4 só falta o `kubectl apply` formal, mas na prática já rodou
junto com a validação da d5). **Só o Bloco B segue bloqueado**, esperando a
validação ponta a ponta da d2 (ver `d2-doacao-worker.md`) — é o único gargalo
real que resta antes do Bloco E. Bloco E é sempre o último passo, só depois
que os outros quatro estiverem prontos.

## Andamento

- [ ] **Bloco A**
  - [ ] Diagrama de arquitetura exportado para o formato de entrega
  - [ ] PDF de justificativa de banco de dados
  - [ ] Relatório de entrega — campos fixos (grupo, participantes, Discord,
        link do repositório)
  - [ ] README — esqueleto + seção Identidade/Campanha
- [ ] **Bloco B** (depende de d2 fechada)
  - [ ] README — seção Doação/Worker/RabbitMQ via docker-compose
  - [ ] Roteiro de texto do vídeo: autenticação, criação de campanha,
        simulação de doação
- [ ] **Bloco C** (depende de d3)
  - [ ] README — seção do pipeline (se aplicável)
  - [ ] Vídeo — trecho do pipeline de CI
- [ ] **Bloco D** (depende de d4 + d5)
  - [ ] README — seção de deploy Kubernetes + Grafana
  - [ ] Vídeo — trecho `kubectl get pods` + dashboard Grafana
- [ ] **Bloco E** (depende de A+B+C+D)
  - [ ] Gravação/montagem final do vídeo (máx. 15 min)
  - [ ] Relatório de entrega — fechar com o link do vídeo

## Pendências / dúvidas

- Definir quem fica com cada bloco — ver seção "Divisão por blocos" acima.
  Bloco A não tem dependência nenhuma, então é o melhor ponto de partida
  para quem estiver livre primeiro.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (critérios de
  aceite)
- `../ARQUITETURA.md` — base para o diagrama e a justificativa
