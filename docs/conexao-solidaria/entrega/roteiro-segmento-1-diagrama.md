# Roteiro — Abertura e Segmento 1: Diagrama de Arquitetura

> Uso: texto de apoio para quem for gravar a abertura e este segmento do
> vídeo de demonstração (D6). Não precisa ser lido palavra por palavra — é
> para parafrasear em voz alta, olhando para o diagrama em
> `docs/conexao-solidaria/entrega/diagrama-arquitetura.png` (ou direto no
> mermaid de `../ARQUITETURA.md`) enquanto fala. Duração estimada: 4 a 5
> minutos (abertura + segmento 1) — 619 palavras no texto, ~140
> palavras/minuto numa fala natural com pausas pra apontar o diagrama.
> Se quiser um vídeo mais enxuto, considere cortar/resumir antes de gravar
> (o edital não exige tempo mínimo, só o teto de 15 min pro vídeo inteiro).
> O segmento 1 é o único do vídeo que é só apresentação — sem código, sem
> terminal.

---

## Abertura (apresentação do vídeo)

Oi, pessoal! Eu sou o Gabriel, e esse é o vídeo de demonstração do nosso
projeto pro Hackathon Fase 5 da Pós-Graduação em Arquitetura de Sistemas
.NET da FIAP, Turma 11NETT: a **Conexão Solidária**.

É uma plataforma de gestão de doadores e campanhas de arrecadação pra uma
ONG — um MVP focado em escalabilidade, observabilidade e automação, como
pede o edital. Nos próximos minutos eu vou mostrar a arquitetura da
solução, o pipeline de CI, o cluster Kubernetes rodando com observabilidade
de verdade, e o sistema funcionando de ponta a ponta: autenticação, criação
de campanha e uma doação sendo processada de forma assíncrona.

Vamos começar pela arquitetura.

## Segmento 1 — Diagrama de arquitetura

Nesta parte eu vou explicar a arquitetura da Conexão Solidária, a
plataforma que a gente construiu para conectar doadores a campanhas de
arrecadação de uma ONG. Vou usar esse diagrama aqui como guia, sem entrar
em código — a ideia é mostrar como as peças se encaixam.

Do lado esquerdo temos quem acessa o sistema, agrupado numa caixa só de
"Atores" pra deixar claro que é a mesma categoria de coisa: client-side,
batendo na API via HTTP. Dentro dela, o Doador e o GestorONG se autenticam e
fazem requisições HTTP autenticadas, e o Público em geral acessa só o painel
de transparência, sem precisar de login. Todo esse tráfego chega num único
ponto de entrada, a `conexao-solidaria-api`.

Aqui já vale destacar uma decisão importante: apesar do nome sugerir
"microsserviços" no plural, a nossa arquitetura é um monolito modular com
apenas dois processos que de fato são implantados de forma independente.
O primeiro é essa `conexao-solidaria-api`, que concentra os módulos de Identidade,
Campanha e Doação — cada um organizado em pastas dentro do mesmo
repositório, não em projetos ou repositórios separados. O segundo processo
deployável é o `doacoes-worker`, que eu mostro já já.

Quando uma doação é registrada, a API não atualiza o valor arrecadado da
campanha na hora, dentro da mesma requisição. Em vez disso, ela publica um
evento — o `DoacaoRecebidaEvent` — numa fila do RabbitMQ, que é o broker de
mensageria do projeto. Isso é uma exigência do edital, mas também faz
sentido pela arquitetura em si: a API responde rápido pro usuário, e quem
processa a doação de fato é o Worker, no ritmo dele, com reprocessamento
automático se alguma coisa falhar no meio do caminho.

O `doacoes-worker` consome essa fila e faz duas coisas dentro de uma única
transação: confirma a doação e atualiza o valor arrecadado da campanha
correspondente. Ele acessa o banco diretamente, sem precisar chamar a API
de volta por HTTP.

E por falar em banco: aqui do lado direito temos o Postgres. É importante
frisar que isso é **uma instância só, com três schemas** — identidade,
campanha e doação — e não três bancos de dados separados. Foi uma escolha
deliberada: como os schemas de campanha e doação estão no mesmo banco
físico, o Worker consegue rodar uma transação real, atômica, cobrindo os
dois ao mesmo tempo, sem precisar de outbox pattern ou saga, que seria bem
mais complexidade do que o prazo do hackathon permitia.

Por fim, a parte de observabilidade, que também é uma exigência do
edital e que a gente adicionou nesta versão do diagrama: o Prometheus faz
scrape periódico do endpoint de métricas da `conexao-solidaria-api`, coletando dados
como número de requisições por rota, latência e uso de CPU e memória dos
pods. E o Grafana lê esses dados direto do Prometheus e exibe tudo num
dashboard, que a gente mostra com números reais mais adiante no vídeo, no
segmento do Kubernetes.

Resumindo os pontos-chave: dois serviços deployáveis — API e Worker — um
banco Postgres único com três schemas, RabbitMQ desacoplando a escrita da
doação do processamento assíncrono dela, e Prometheus mais Grafana
cobrindo a observabilidade de ponta a ponta. Essa é a arquitetura que
vamos ver funcionando na prática no resto do vídeo.
