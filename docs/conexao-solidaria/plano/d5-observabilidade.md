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

- [x] Base OpenTelemetry compartilhada (`Infrastructure/Configurations/ObservabilidadeConfiguration.cs`):
      métricas sempre ligadas; traces e logs via OTLP só quando
      `OTEL_EXPORTER_OTLP_ENDPOINT` está setado. Usada pela API e pelo Worker
      (instrumentação de HttpClient, runtime .NET e MassTransit)
- [x] Endpoint `/health` na `ConexaoSolidaria.API` (Postgres via
      `AddDbContextCheck`; RabbitMQ via health check que o MassTransit já
      registra) + `/health/live` para liveness
- [x] Endpoint `/metrics` (Prometheus format — exporter Prometheus do
      OpenTelemetry, `OpenTelemetry.Exporter.Prometheus.AspNetCore`, no lugar do
      `prometheus-net` para não ter duas stacks de métricas)
- [x] Readiness/liveness probes no deployment da API
- [x] Deployment do Prometheus no cluster (`k8s/prometheus`, scrape dos pods
      da API via Service headless `conexao-solidaria-api-metrics`)
- [x] Deployment do Grafana no cluster (`k8s/grafana`, datasource e dashboard
      provisionados por ConfigMap, Ingress em `grafana.conexaosolidaria.local` —
      o Ingress ainda não foi testado, a validação foi via port-forward)
- [x] Pelo menos 1 dashboard configurado — "Conexão Solidária — API":
      requisições HTTP (por rota/status), taxa de 5xx, latência p50/p95/p99,
      CPU e memória por pod, heap do GC, threads
- [x] Validado no Kubernetes do Docker Desktop (2026-09-27): target da API
      `UP` no Prometheus, `/health` Healthy, dashboard com tráfego real
      (requisições por rota/status, latência, CPU, memória, GC, threads)
- [ ] Print do dashboard para o vídeo/README da D6

## Pendências / dúvidas

- O Worker não expõe `/metrics` (é um host sem HTTP) — as métricas dele só
  saem via OTLP quando houver coletor. Se precisar dele no Grafana sem
  coletor, dá para usar `OpenTelemetry.Exporter.Prometheus.HttpListener`

- O edital cita "Zabbix e Grafana" no título da seção mas só exige Grafana
  no texto — confirmar se o grupo quer usar Zabbix também ou só Prometheus +
  Grafana (mais simples, mesmo resultado exigido)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
