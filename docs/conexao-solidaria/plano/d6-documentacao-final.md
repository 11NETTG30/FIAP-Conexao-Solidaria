# D6 — Documentação final e vídeo

Depende de tudo (d0–d5). É a última demanda antes da entrega.

**2026-09-28 — revisão da divisão**: a primeira versão desta doc dividia o
trabalho em blocos por dependência (A/B/C/D), inclusive fatiando o README
em 4 seções incrementais — uma por bloco, cada uma escrita conforme a
demanda correspondente destravava. Isso fazia sentido enquanto d2/d3/d4/d5
iam ficando prontas em momentos diferentes. Agora que **todas já estão
marcadas como concluídas ao mesmo tempo**, fatiar o README deixou de ter
propósito — é só risco de ficar inconsistente sem ganho nenhum, já que
qualquer pessoa pode escrever o README inteiro numa passada só, com tudo
já pronto pra referenciar. Reestruturado abaixo: tarefas de documentação
como itens independentes (sem fragmentar entre si), e só a **gravação do
vídeo** continua dividida — ali sim faz sentido, porque cada segmento
mostra uma parte diferente do sistema e quem implementou aquela parte é
quem demonstra melhor.

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

## Divisão do trabalho

### Documentação — tarefas independentes, cada uma por 1 pessoa, sem
depender umas das outras

- **Diagrama de arquitetura** (item 2) — exportar o mermaid de
  `../ARQUITETURA.md` para o formato de entrega. **Precisa de um ajuste
  antes de exportar**: o diagrama atual só mostra Doador/GestorONG/Público
  → API → RabbitMQ → Worker → Postgres — **não tem Prometheus nem Grafana
  desenhados**, e o edital exige explicitamente que o diagrama mostre "os
  microsserviços, os bancos de dados, o broker de mensageria **e as
  ferramentas de observabilidade**". Adicionar os nós de Prometheus/Grafana
  (scraping da API, conforme `k8s/prometheus/` e `k8s/grafana/`) antes de
  exportar.
- **PDF de justificativa de banco** (item 3) — conteúdo técnico já escrito
  em `../ARQUITETURA.md`, seção "Banco de dados" (1 Postgres, 3 schemas,
  por que não bancos separados); é reescrever em prosa e exportar
- **README completo** (item 1) — pré-requisitos, estrutura de pastas, e
  como subir tudo localmente: Identidade + Campanha (docker-compose básico),
  Doação/Worker/RabbitMQ, pipeline de CI (se fizer sentido documentar) e
  deploy em Kubernetes + acesso ao Grafana. Uma pessoa só escreve isso
  inteiro, numa passada — todo o conteúdo técnico já existe implementado e
  documentado nos respectivos `d*.md`, é consolidar
- **Relatório de entrega — campos fixos** (item 5, parcial): nome do grupo,
  participantes, usernames no Discord, link do repositório — só o grupo tem
  essa informação, não depende de nada técnico
- **Roteiro de texto do vídeo** — escrever o passo a passo/narração de cada
  trecho do roteiro obrigatório (pode ser feito por quem vai gravar cada
  segmento, ver abaixo, ou por uma pessoa só revisando tudo)

### Gravação do vídeo — dividida por segmento (aqui sim faz sentido
paralelizar, por pessoa/conhecimento de cada parte)

1. Explicação do diagrama de arquitetura
2. Pipeline de CI executando e gerando a imagem Docker
3. Terminal com `kubectl get pods` + dashboard Grafana com dados reais
4. Fluxo funcional: autenticação (Postman/Swagger + JWT) → criação de
   campanha → simulação de doação (payload → RabbitMQ → valor atualizado
   na API pública)

⚠️ **Risco no segmento 4**: a d2 (Doação/Worker) foi marcada como concluída
por decisão do grupo, mas a validação ponta a ponta contra
Postgres/RabbitMQ reais nunca rodou de fato (ver `d2-doacao-worker.md`).
Esse segmento do vídeo só sai bem se o fluxo realmente funcionar na
prática — **testar antes de agendar a gravação**, não durante.

### Bloco final — sequencial, depende de tudo acima

- Montagem do vídeo completo (máx. 15 min), juntando os 4 segmentos
- Fechar o relatório de entrega com o link do vídeo publicado

## Andamento

- [ ] Diagrama de arquitetura — ajustar (incluir Prometheus/Grafana) e
      exportar
- [ ] PDF de justificativa de banco de dados
- [ ] README completo
- [ ] Relatório de entrega — campos fixos (grupo, participantes, Discord,
      link do repositório)
- [ ] Roteiro de texto do vídeo
- [ ] Vídeo — segmento 1: diagrama de arquitetura
- [ ] Vídeo — segmento 2: pipeline de CI
- [ ] Vídeo — segmento 3: `kubectl get pods` + Grafana
- [ ] Vídeo — segmento 4: fluxo funcional completo (testar antes de gravar)
- [ ] Montagem final do vídeo
- [ ] Relatório de entrega — fechar com o link do vídeo

## Pendências / dúvidas

- Definir quem fica com cada item — tudo listado acima já está destravado,
  não tem mais dependência entre demandas segurando nada.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (critérios de
  aceite)
- `../ARQUITETURA.md` — base para o diagrama e a justificativa
