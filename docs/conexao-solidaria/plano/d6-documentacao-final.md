# D6 — Documentação final e vídeo

Depende de tudo (d0–d5). É a última demanda antes da entrega. Detalhado
abaixo em blocos, para dividir entre o grupo — desde 2026-09-28 todos os
blocos A/B/C/D já estão destravados (ver "Divisão por blocos").

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

### Bloco B — **destravado** (d2 marcada como fechada em 2026-09-28, por
decisão, sem a validação ponta a ponta ter rodado de fato — ver
`d2-doacao-worker.md`)

- Seção do README sobre RabbitMQ/Worker/fluxo de doação via
  `docker-compose` (item 1)
- Roteiro de texto (não a gravação) dos trechos "Autenticação via
  Postman/Swagger", "Criação de campanha" e "Simulação de doação" do vídeo
  (item 4, sub-itens d/e/f do roteiro) — pode ser escrito já

⚠️ **Risco que continua real, mesmo com a d2 marcada como feita**: o
roteiro obrigatório do vídeo (item 4, sub-item f) exige mostrar o payload
da doação, a mensagem passando pelo RabbitMQ e o valor da campanha
atualizado de verdade — isso só é possível se o fluxo funcionar na prática.
Como ninguém rodou essa validação ainda, existe a chance real de encontrar
um problema só na hora de gravar. Recomendado: reservar tempo para rodar
`docker-compose up` (Postgres + RabbitMQ + API + Worker) e testar o fluxo
completo **antes** de agendar a gravação do Bloco E, não durante.

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

**Ordem prática** (atualizado em 2026-09-28): **todos os blocos A/B/C/D estão
destravados** — d2, d3 e d5 estão marcadas como fechadas, d4 tem os
manifests prontos. Dá para dividir os quatro em paralelo agora. O único
cuidado real é o risco descrito no Bloco B: como a d2 nunca rodou de fato
contra Postgres/RabbitMQ reais, vale testar esse fluxo antes de marcar o
Bloco B como pronto de verdade, e principalmente antes de agendar a
gravação do Bloco E — que é sempre o último passo, só depois que os outros
quatro estiverem prontos.

## Andamento

- [ ] **Bloco A**
  - [ ] Diagrama de arquitetura exportado para o formato de entrega
  - [ ] PDF de justificativa de banco de dados
  - [ ] Relatório de entrega — campos fixos (grupo, participantes, Discord,
        link do repositório)
  - [ ] README — esqueleto + seção Identidade/Campanha
- [ ] **Bloco B** (destravado — ver aviso de risco acima)
  - [ ] README — seção Doação/Worker/RabbitMQ via docker-compose
  - [ ] Roteiro de texto do vídeo: autenticação, criação de campanha,
        simulação de doação
  - [ ] Testar o fluxo de doação de ponta a ponta pelo menos uma vez antes
        de agendar a gravação (recomendado, não bloqueante para o resto do
        bloco)
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
