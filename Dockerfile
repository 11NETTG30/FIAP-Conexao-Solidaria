# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# CA extra opcional (ex.: proxy de egress de um sandbox de build) — a pasta
# normalmente está vazia e essa etapa não faz nada nesse caso.
# Ver docker/certs/README.md.
COPY docker/certs/ /usr/local/share/ca-certificates/extra/
RUN update-ca-certificates

COPY FiapCloudGames.slnx ./
COPY src/FCG.API/FCG.API.csproj src/FCG.API/
COPY src/FCG.Application/FCG.Application.csproj src/FCG.Application/
COPY src/FCG.Domain/FCG.Domain.csproj src/FCG.Domain/
COPY src/FCG.Infrastructure/FCG.Infrastructure.csproj src/FCG.Infrastructure/
COPY src/FCG.IoC/FCG.IoC.csproj src/FCG.IoC/
RUN dotnet restore src/FCG.API/FCG.API.csproj

COPY src/ src/
RUN dotnet publish src/FCG.API/FCG.API.csproj -c Release -o /app/publish --no-restore

# Estágio usado só pelo serviço "migrate" do docker-compose: aplica as
# migrations do EF Core contra o Postgres antes da API subir. Com mais de um
# DbContext o "database update" exige --context, então roda um por módulo.
FROM build AS migrate
COPY .config/dotnet-tools.json .config/dotnet-tools.json
RUN dotnet tool restore
ENV MIGRATION_CONTEXTS="IdentidadeDbContext CampanhaDbContext"
ENTRYPOINT ["/bin/sh", "-c", "set -e; for context in $MIGRATION_CONTEXTS; do \
    dotnet tool run dotnet-ef database update --context $context \
    --project src/FCG.Infrastructure/FCG.Infrastructure.csproj \
    --startup-project src/FCG.API/FCG.API.csproj \
    --configuration Release; done"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FCG.API.dll"]
