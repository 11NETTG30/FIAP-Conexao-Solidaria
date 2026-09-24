# D6 — Documentação final e vídeo

Depende de tudo (d0–d5). É a última demanda antes da entrega.

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

## Andamento

- [ ] Atualizar README.md do repositório com setup completo
- [ ] Exportar diagrama de arquitetura para entrega
- [ ] Escrever justificativa de escolha de banco (Postgres, schema por
      módulo) em PDF
- [ ] Gravar vídeo seguindo o roteiro obrigatório
- [ ] Montar relatório de entrega

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo (critérios de aceite)
- `../ARQUITETURA.md` — base para o diagrama e a justificativa
