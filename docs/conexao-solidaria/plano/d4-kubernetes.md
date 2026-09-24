# D4 — Manifests Kubernetes

Depende de D3 (precisa das imagens Docker existindo/publicadas).

## Objetivo

Rodar a solução inteira num cluster Kubernetes (Minikube, Kind ou Docker
Desktop K8s — exigência do edital, qualquer um serve), com os arquivos
`.yaml` entregáveis.

## Instruções

Herda tudo de `visao-geral.md`.

## Contexto

Exigência do edital: cluster K8s com `Deployments`, `Services` e
`ConfigMaps` entregues. Processos a orquestrar (ver
`../ARQUITETURA.md`):

- `fcg-api` — Deployment + Service (exposto)
- `doacoes-worker` — Deployment (sem Service, não recebe tráfego HTTP)
- Postgres — pode ser Deployment simples com PVC, ou StatefulSet
- RabbitMQ — idem

## Andamento

- [ ] `k8s/fcg-api-deployment.yaml` + `k8s/fcg-api-service.yaml`
- [ ] `k8s/doacoes-worker-deployment.yaml`
- [ ] `k8s/postgres-deployment.yaml` (+ PVC)
- [ ] `k8s/rabbitmq-deployment.yaml`
- [ ] `k8s/configmap.yaml` com as variáveis de ambiente não sensíveis
      (mover o que hoje está em `appsettings.json`/`.env` para cá, conforme
      já é prática registrada em
      `/home/claude` → nota de aprendizado do projeto FCG anterior: "mover
      configs para fora de appsettings.json para ConfigMaps/Secrets")
- [ ] Secret para a connection string e o `JwtSettings:Secret`
- [ ] Testar `kubectl apply -f k8s/` localmente de ponta a ponta

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
