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

### Roteiro detalhado por segmento

Separado em duas colunas por segmento: **Obrigatório** é texto do edital,
literal (o que o vídeo *precisa* mostrar); **Sugestão de execução** é
elaboração desta sessão sobre como executar bem — não é exigência, é só
para não gravar do zero sem plano nenhum.

**Segmento 1 — Diagrama de arquitetura**
- Obrigatório (edital): "Explicação do Diagrama de Arquitetura." A
  instrução geral do vídeo também vale aqui: "não deve focar em ler código
  linha a linha, mas sim em comprovar a arquitetura e o funcionamento."
- Sugestão de execução: apontar no diagrama os 2 serviços deployáveis (API
  e Worker), 1 Postgres com 3 schemas (não 3 bancos), RabbitMQ como broker
  entre API e Worker, Prometheus fazendo scrape da API e Grafana lendo do
  Prometheus.

**Segmento 2 — Pipeline de CI**
- Obrigatório (edital): "Demonstração do Pipeline de CI executando e
  gerando a imagem Docker com sucesso."
- Sugestão de execução: abrir a aba Actions do GitHub, mostrar o workflow
  `docker-publish.yml` rodando ou uma execução recente com sucesso; mostrar
  os 2 jobs (`test` → `build-and-push`); opcional mostrar o pacote
  publicado no GHCR.

**Segmento 3 — Kubernetes + Grafana**
- Obrigatório (edital): "Terminal mostrando os pods rodando no Kubernetes
  (`kubectl get pods`) e o dashboard do Grafana exibindo os dados em tempo
  real."
- Sugestão de execução: mostrar os painéis do dashboard "Conexão
  Solidária — API" (requisições por rota/status, latência p50/p95/p99,
  CPU/memória por pod, GC, threads); gerar algum tráfego real na API antes
  de gravar para o dashboard não aparecer zerado.

**Segmento 4 — Funcionamento**
- Obrigatório (edital):
  i. "Autenticação via Postman/Swagger e obtenção do token JWT."
  ii. "Criação de uma campanha."
  iii. "Simulação de uma Doação: mostrar o payload sendo enviado; em
       seguida abrir a interface do RabbitMQ/Kafka mostrando a mensagem
       passando pela fila e, por fim, consultar a API pública para provar
       que o valor da campanha foi atualizado pelo Worker."
- Sugestão de execução: login com o admin de seed para (i); `POST
  /api/campanhas` mostrando payload e resposta `201` com `id` para (ii);
  `POST /api/doacoes` mostrando o payload, abrir a interface de management
  do RabbitMQ (`localhost:15672`) mostrando a mensagem na fila
  `doacao-recebida`, depois `GET /api/campanhas` mostrando o
  `ValorArrecadado` atualizado para (iii).

**Fechamento** — isto **não está no edital**, é sugestão livre: voltar no
diagrama por alguns segundos e recapitular verbalmente os requisitos
técnicos batidos (microsserviços, mensageria assíncrona, observabilidade,
CI) antes de encerrar.

### Bloco final — sequencial, depende de tudo acima

- Montagem do vídeo completo (máx. 15 min), juntando os 4 segmentos
- Fechar o relatório de entrega com o link do vídeo publicado

## Andamento

- [x] Diagrama de arquitetura — ajustar (incluir Prometheus/Grafana) e
      exportar (**Gabriel**): mermaid atualizado em `../ARQUITETURA.md`
      (nós `Prometheus` e `Grafana` adicionados) e exportado para
      `../entrega/diagrama-arquitetura.png` via `@mermaid-js/mermaid-cli`
- [x] Diagrama de arquitetura — revisão visual (**Gabriel**, 2026-09-29):
      dono do projeto achou o diagrama anterior visualmente pobre (caixas
      brancas uniformes, atores soltos). Refeito no próprio mermaid de
      `../ARQUITETURA.md` (Opção A — sem precisar de SVG customizado):
      `Doador`/`GestorONG`/`Publico` agrupados num `subgraph "Atores (HTTP)"`
      com borda tracejada arredondada; cor por camada via `classDef`/`class`
      (azul = atores, verde = aplicação `fcg-api`/`doacoes-worker`, laranja =
      mensageria, amarelo = dados, roxo = observabilidade); formas por tipo
      de nó (hexágono para o RabbitMQ, cilindro para o Postgres, círculo para
      Prometheus/Grafana, retângulo de sub-rotina para os dois serviços
      deployáveis); rótulo do Postgres corrigido para citar os 3 schemas
      (`identidade`/`campanha`/`doacao` — antes só citava campanha+doacao).
      Nenhum componente, seta ou rótulo de seta existente foi removido.
      Reexportado para `../entrega/diagrama-arquitetura.png` com
      `@mermaid-js/mermaid-cli` (`-s 3 -b white`, puppeteer `--no-sandbox`
      por rodar como root), conferido visualmente após cada iteração.
      Roteiro do segmento 1 (`../entrega/roteiro-segmento-1-diagrama.md`)
      ajustado com uma frase sobre o agrupamento visual dos atores.
- [x] PDF de justificativa de banco de dados (**Gabriel**) — feito em
      2026-09-29, revisado no mesmo dia para caber em 1 página (a primeira
      versão, com 3 páginas, incluía "papel da fila" e "idempotência" —
      conteúdo de mensageria, não de banco de dados, fora do que o edital
      pediu literalmente). Dois arquivos em `../entrega/`:
      - `justificativa-banco-dados.md`/`.pdf` — **versão de entrega**, 1
        página, só os 2 pontos centrais: por que PostgreSQL, por que 1
        instância com 3 schemas em vez de 3 bancos separados
      - `justificativa-banco-dados-completa.md`/`.pdf` — versão original
        de 3 páginas, mantida como material de apoio para quem quiser o
        raciocínio completo (inclui fila e idempotência)
      Gerado com reportlab (pandoc/wkhtmltopdf/LaTeX não estavam
      disponíveis no ambiente; reportlab via pip funcionou de primeira e
      não deixa dependência pesada no repositório).
      **Ajuste de 2026-09-29 (a pedido do Gabriel)**: a versão de entrega
      (`justificativa-banco-dados.pdf`) estava ocupando 2 páginas e com
      pouco respiro visual entre título, subtítulo e o primeiro parágrafo
      de cada seção. Reequilibrado layout para caber em 1 página com bem
      mais respiro entre título/subtítulo/texto — margens superior/inferior
      reduzidas para 1cm (abaixo do padrão ABNT de 3cm/2cm, decisão
      explícita do Gabriel, que topou abrir mão do padrão estrito nesse
      ponto) e `spaceAfter`/`spaceBefore` do título, subtítulo e títulos de
      seção aumentados. Não foi preciso cortar nenhuma frase do `.md`. Os
      mesmos ajustes de espaçamento (mantendo margens ABNT completas, já
      que essa versão não precisa caber em 1 página) foram aplicados em
      `justificativa-banco-dados-completa.pdf` para manter consistência
      visual entre os dois PDFs.
- [x] README completo (**Gabriel**) — reescrito do zero, cobrindo visão
      geral, arquitetura, pré-requisitos, subida via docker-compose,
      credenciais de teste, endpoints/fluxos principais, testes (unitários
      + smoke E2E), CI/CD e deploy em Kubernetes; sinaliza explicitamente
      onde falta validação ponta a ponta (fluxo doação → worker, `kubectl
      apply -f k8s/` completo). **Nota da sessão orquestradora**: o README
      foi escrito com base num estado do repositório anterior ao merge da
      d0.1 (PR #18), então a menção original de que "o CPF do doador ainda
      não está implementado" estava desatualizada — corrigida no commit
      seguinte a este merge, já que a d0.1 foi mergeada nesse meio tempo.
- [ ] Relatório de entrega — campos fixos (grupo, participantes, Discord,
      link do repositório) (**Gabriel**)
- [x] Roteiro de texto do vídeo — segmento 1 (**Gabriel**):
      `../entrega/roteiro-segmento-1-diagrama.md`. **Segmentos 2, 3 e 4
      seguem sem dono** (roteiro de texto e gravação)
- [ ] Vídeo — segmento 1: diagrama de arquitetura
- [ ] Vídeo — segmento 2: pipeline de CI
- [ ] Vídeo — segmento 3: `kubectl get pods` + Grafana
- [ ] Vídeo — segmento 4: fluxo funcional completo (testar antes de gravar)
- [ ] Montagem final do vídeo
- [ ] Relatório de entrega — fechar com o link do vídeo (**Gabriel**)

## Pendências / dúvidas

- 2026-09-28 — Gabriel assumiu diagrama, PDF de banco, README completo,
  relatório de entrega e o roteiro de texto do segmento 1. **Falta
  definir quem fica com**: roteiro de texto dos segmentos 2/3/4, e a
  gravação dos 4 segmentos do vídeo (lembrando que 3 e 4 precisam ser
  gravados juntos, pelo mesmo grupo/pessoa — ver "Gravação do vídeo"
  acima) + a montagem final.
- 2026-09-29 — Diagrama de arquitetura (com Prometheus/Grafana) e roteiro
  de texto do segmento 1 concluídos, ver `../entrega/`. **Continua em
  aberto**: quem escreve o roteiro de texto dos segmentos 2, 3 e 4, e quem
  grava cada segmento do vídeo.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (critérios de
  aceite)
- `../ARQUITETURA.md` — base para o diagrama e a justificativa
