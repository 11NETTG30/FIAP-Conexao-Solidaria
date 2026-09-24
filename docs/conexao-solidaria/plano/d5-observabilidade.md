# D5 — Observabilidade

Depende de D4 (precisa dos pods rodando no cluster).

## Objetivo

Expor métricas reais da aplicação e ter um dashboard Grafana configurado —
exigência obrigatória do edital.

## Instruções

Herda tudo de `visao-geral.md`.

## Contexto

Exigência do edital: "A aplicação deve expor métricas de saúde (`/health` ou
`/metrics`). O Grafana deve possuir pelo menos um dashboard configurado
exibindo métricas reais da aplicação rodando (ex.: consumo de CPU/Memória
dos pods ou contagem de requisições HTTP)."

## Andamento

- [ ] Endpoint `/health` na `FCG.API` (`Microsoft.Extensions.Diagnostics.HealthChecks`,
      checando Postgres e RabbitMQ)
- [ ] Endpoint `/metrics` (Prometheus format — `prometheus-net.AspNetCore`
      ou equivalente)
- [ ] Deployment do Prometheus no cluster (scrape dos pods)
- [ ] Deployment do Grafana no cluster
- [ ] Pelo menos 1 dashboard configurado (CPU/memória dos pods ou contagem
      de requisições HTTP)

## Pendências / dúvidas

- O edital cita "Zabbix e Grafana" no título da seção mas só exige Grafana
  no texto — confirmar se o grupo quer usar Zabbix também ou só Prometheus +
  Grafana (mais simples, mesmo resultado exigido)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
