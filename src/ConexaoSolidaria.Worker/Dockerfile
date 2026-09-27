# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# CA extra opcional (ex.: proxy de egress de um sandbox de build) — a pasta
# normalmente está vazia e essa etapa não faz nada nesse caso.
# Ver docker/certs/README.md.
COPY docker/certs/ /usr/local/share/ca-certificates/extra/
RUN update-ca-certificates

COPY ConexaoSolidaria.slnx ./
COPY src/ConexaoSolidaria.Application/ConexaoSolidaria.Application.csproj src/ConexaoSolidaria.Application/
COPY src/ConexaoSolidaria.Domain/ConexaoSolidaria.Domain.csproj src/ConexaoSolidaria.Domain/
COPY src/ConexaoSolidaria.Infrastructure/ConexaoSolidaria.Infrastructure.csproj src/ConexaoSolidaria.Infrastructure/
COPY src/ConexaoSolidaria.Worker/ConexaoSolidaria.Worker.csproj src/ConexaoSolidaria.Worker/
RUN dotnet restore src/ConexaoSolidaria.Worker/ConexaoSolidaria.Worker.csproj

COPY src/ src/
RUN dotnet publish src/ConexaoSolidaria.Worker/ConexaoSolidaria.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConexaoSolidaria.Worker.dll"]
