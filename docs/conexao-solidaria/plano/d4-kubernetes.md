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

- [x] Estrutura base criada em `k8s/` no próprio monorepo, sem copiar o
      desenho antigo com Kong e microsserviços extras
- [x] `namespace.yaml`
- [x] API: `k8s/conexao-solidaria-api/{configmap,secret.example,deployment,service}.yaml`
- [x] Worker: `k8s/doacoes-worker/{configmap,secret.example,deployment}.yaml`
- [x] `ingress.yaml` apontando para a API
- [x] Postgres: `k8s/postgres/{secret.example,pvc,deployment,service}.yaml`
- [x] RabbitMQ: `k8s/rabbitmq/{secret.example,pvc,deployment,service}.yaml`
- [ ] Testar `kubectl apply -f k8s/` localmente de ponta a ponta

## Pendências / dúvidas

Nenhuma no momento.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
