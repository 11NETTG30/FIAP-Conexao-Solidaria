# Kubernetes

Estrutura de manifests da solução Conexão Solidária no mesmo monorepo da aplicação.

## Escopo inicial

- `conexao-solidaria-api`: Deployment + Service + Ingress
- `doacoes-worker`: Deployment sem Service
- `postgres`: Deployment com PVC
- `rabbitmq`: Deployment com PVC
- `prometheus`: Deployment + Service, fazendo scrape do `/metrics` da API
- `grafana`: Deployment + Service + Ingress, com datasource e dashboard provisionados
- `configmap` e `secret` por ambiente/serviço

## Pré-requisitos

- Docker
- `kubectl`
- `kind`

## Voltar para o cluster local

Se o `kubectl` estiver apontando para a Azure, volte para o contexto local:

```bash
kubectl config get-contexts
kubectl config use-context kind-conexao-solidaria
```

Se o cluster local ainda não existir, crie com:

```bash
kind create cluster --name conexao-solidaria
```

## Habilitar Ingress no cluster local

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml
kubectl wait -n ingress-nginx --for=condition=Available deployment/ingress-nginx-controller --timeout=5m
```

## Arquivo hosts

Adicione no sistema:

```text
127.0.0.1 api.conexaosolidaria.local
127.0.0.1 rabbitmq.conexaosolidaria.local
127.0.0.1 grafana.conexaosolidaria.local
```

## Secrets locais

Antes de aplicar, copie os exemplos para os nomes reais:

```bash
cp k8s/conexao-solidaria-api/secret.example.yaml k8s/conexao-solidaria-api/secret.yaml
cp k8s/doacoes-worker/secret.example.yaml k8s/doacoes-worker/secret.yaml
cp k8s/postgres/secret.example.yaml k8s/postgres/secret.yaml
cp k8s/rabbitmq/secret.example.yaml k8s/rabbitmq/secret.yaml
cp k8s/grafana/secret.example.yaml k8s/grafana/secret.yaml
```

Observação: o `JwtSettings__Secret` da API precisa ter no mínimo 32 caracteres,
ou a aplicação falha na validação de startup.

## Imagens locais

Antes de aplicar os deployments, construa as imagens e carregue no cluster local:

```bash
docker build -t conexao-solidaria-api:local -f src/ConexaoSolidaria.API/Dockerfile .
docker build -t doacoes-worker:local -f src/ConexaoSolidaria.Worker/Dockerfile .

kind load docker-image conexao-solidaria-api:local --name conexao-solidaria
kind load docker-image doacoes-worker:local --name conexao-solidaria
```

## Aplicação dos manifests

Ordem recomendada:

```bash
kubectl apply -f k8s/namespace.yaml

kubectl apply -f k8s/postgres/secret.yaml
kubectl apply -f k8s/postgres/pvc.yaml
kubectl apply -f k8s/postgres/service.yaml
kubectl apply -f k8s/postgres/deployment.yaml

kubectl apply -f k8s/rabbitmq/secret.yaml
kubectl apply -f k8s/rabbitmq/pvc.yaml
kubectl apply -f k8s/rabbitmq/service.yaml
kubectl apply -f k8s/rabbitmq/deployment.yaml

kubectl apply -f k8s/conexao-solidaria-api/configmap.yaml
kubectl apply -f k8s/conexao-solidaria-api/secret.yaml
kubectl apply -f k8s/conexao-solidaria-api/service.yaml
kubectl apply -f k8s/conexao-solidaria-api/service-metrics.yaml
kubectl apply -f k8s/conexao-solidaria-api/deployment.yaml

kubectl apply -f k8s/doacoes-worker/configmap.yaml
kubectl apply -f k8s/doacoes-worker/secret.yaml
kubectl apply -f k8s/doacoes-worker/deployment.yaml

kubectl apply -f k8s/prometheus

kubectl apply -f k8s/grafana/secret.yaml
kubectl apply -f k8s/grafana/configmap-provisioning.yaml
kubectl apply -f k8s/grafana/configmap-dashboards.yaml
kubectl apply -f k8s/grafana/service.yaml
kubectl apply -f k8s/grafana/deployment.yaml

kubectl apply -f k8s/ingress.yaml
```

## Verificação

```bash
kubectl get pods -n conexao-solidaria
kubectl get svc -n conexao-solidaria
kubectl get ingress -n conexao-solidaria
```

Se quiser acessar a API sem Ingress, use port-forward:

```bash
kubectl port-forward svc/conexao-solidaria-api 8080:80 -n conexao-solidaria
```

Para acessar o RabbitMQ Management sem Ingress, use:

```bash
kubectl port-forward svc/rabbitmq-management 15672:15672 -n conexao-solidaria
```

## Observabilidade

A API expõe:

- `/metrics` — métricas no formato Prometheus (OpenTelemetry): requisições
  HTTP, latência, CPU/memória do processo, GC, thread pool, MassTransit
- `/health` — readiness: Postgres + RabbitMQ (usado pela `readinessProbe`)
- `/health/live` — liveness: só confirma que o processo responde

O Prometheus descobre cada pod da API pelo Service headless
`conexao-solidaria-api-metrics` e o Grafana já sobe com o datasource e o
dashboard **Conexão Solidária — API** (pasta "Conexão Solidária").

Acesso ao Grafana: `http://grafana.conexaosolidaria.local` (login do
`grafana/secret.yaml`), ou sem Ingress:

```bash
kubectl port-forward svc/grafana 3000:3000 -n conexao-solidaria
```

Para conferir os targets do Prometheus (`Status → Targets`):

```bash
kubectl port-forward svc/prometheus 9090:9090 -n conexao-solidaria
```

Traces e logs via OTLP ficam desligados por padrão; para ligar, descomente
`OTEL_EXPORTER_OTLP_ENDPOINT` nos configmaps da API/Worker apontando para um
coletor OpenTelemetry.

## Derrubar o ambiente

Para remover todos os recursos aplicados no namespace:

```bash
kubectl delete -f k8s/ingress.yaml --ignore-not-found
kubectl delete -f k8s/grafana --ignore-not-found
kubectl delete -f k8s/prometheus --ignore-not-found
kubectl delete -f k8s/doacoes-worker --ignore-not-found
kubectl delete -f k8s/conexao-solidaria-api --ignore-not-found
kubectl delete -f k8s/rabbitmq --ignore-not-found
kubectl delete -f k8s/postgres --ignore-not-found
kubectl delete -f k8s/namespace.yaml --ignore-not-found
```

Se quiser apagar também o cluster local inteiro do kind:

```bash
kind delete cluster --name conexao-solidaria
```
