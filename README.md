# Conexão Solidária

## 📚 Sobre o Projeto

Este repositório implementa o MVP da plataforma **Conexão Solidária**,
desenvolvido como Hackathon da Pós-Graduação em Arquitetura de Sistemas
.NET da FIAP, **Turma 11NETT – Grupo 30**.

A Conexão Solidária é uma plataforma de gestão de doações para ONGs:
permite que uma organização cadastre e gerencie campanhas de arrecadação,
que doadores se cadastrem e contribuam com essas campanhas, e expõe um
painel de transparência público com o andamento de cada campanha.

---

## 🎯 Objetivos do Hackathon

Os principais objetivos deste projeto são:

- Autenticação e autorização via **JWT**, com dois perfis: `GestorONG`
  (gerencia campanhas) e `Doador` (contribui com campanhas)
- Gestão de **campanhas de arrecadação** (criação/edição, restrita a
  `GestorONG`) e cadastro público de **doadores**
- Painel de transparência público (campanhas ativas e valor arrecadado)
- Processamento assíncrono de doações via mensageria, sem atualização
  direta do valor arrecadado na mesma requisição da intenção de doação
- Persistência com **Entity Framework Core**
- Documentação da API com **Swagger/OpenAPI**
- Testes unitários e de mutação

Mais detalhes sobre o desenho da solução em
`docs/conexao-solidaria/ARQUITETURA.md`.

---

## 🛠️ Tecnologias Utilizadas

| Categoria                | Tecnologia / Ferramenta                                             |
|--------------------------|---------------------------------------------------------------------|
| Plataforma               | .NET 10                                                             |
| Framework Web            | ASP.NET                                                             |
| Linguagem de Programação | C# 14                                                               |
| ORM / Persistência       | Entity Framework Core com Migrations                                |
| Banco de Dados           | PostgreSQL                                                          |
| Documentação API         | OpenAPI                                                             |
| Documentação API (UI)    | Swagger e Scalar                                                    |
| Autenticação             | JWT (JSON Web Tokens) + RefreshToken rotativo                       |
| Hash senha               | Argon2id com 19 MiB de memória, 2 iterações e grau 1 de paralelismo |
| Monitoramento            | New Relic (.NET Agent)                                              |
| Testes Unitários         | xUnit                                                               |
| Testes de Mutação        | Stryker.NET                                                         |
| BDD                      | Reqnroll, NUnit, Moq                                                |

## 🚀 Setup Inicial

### 1. Configurar Variáveis de Ambiente do docker-compose

```bash
# Copie o arquivo de exemplo
cp .env.example .env

# Edite o .env com suas credenciais
```

### 2. Configurar Variáveis de Ambiente do projeto (src\FCG.API)

```bash
# Copie o arquivo de exemplo
cp .env.example .env

# Edite o .env com suas credenciais
```

### 3. Comandos Docker / Banco de Dados

```bash
# Inicia PostgreSQL e PgAdmin (obs: necessário instalar e abrir o Docker Desktop)
docker-compose up -d

# Verifica se subiu
docker-compose ps

# Ver logs
docker-compose logs -f postgres

# Parar
docker-compose down

# Parar e remover volumes (cuidado: apaga os dados!)
docker-compose down -v

# Acessar o PostgreSQL
docker exec -it conexao-solidaria-postgres psql -U conexaosolidaria -d conexao_solidaria

# Caso dê erro para subir o container postgres, rode o seguinte comando no terminal:
wsl dos2unix scripts/init-database.sh
```

### 4. Aplicar Migrations

No console do Gerenciador de Pacotes, selecione o projeto padrão (ex: `src\FCG.Infrastructure`) e execute os comandos:

```powershell
# Criar uma nova migration
Add-Migration InitialIdentidade -Context IdentidadeDbContext -OutputDir Identidade/Persistence/Migrations

# Aplicar as alterações no banco de dados (um comando por módulo)
Update-Database -Context IdentidadeDbContext
Update-Database -Context CampanhaDbContext
```

### 5. Execução inicial da Aplicação

```bash
# Para iniciar o banco de dados, no PowerShell
docker-compose up -d

# Aplicar as alterações no banco de dados, no Console do Gerenciador de Pacotes
Update-Database -Context IdentidadeDbContext
Update-Database -Context CampanhaDbContext
```

Rodar a API (FCG.API)

Acesse: https://localhost:5001/swagger

## Dados do administrador para login:
```json
{
	"email": "admin@conexaosolidaria.com.br",
	"senha": "Admin@123"
}
```

## 🕒 Datas na API

Todas as datas trafegam em **UTC** (ISO 8601). Uma data enviada sem fuso — ex.:
`2026-12-31T23:59:59` — é interpretada como UTC, ou seja, 20:59 no horário de
Brasília. Para indicar o horário local, envie o fuso: `2026-12-31T23:59:59-03:00`.

## 📊 Acessar PgAdmin

- **URL:** http://localhost:5050
- **Email:** (conforme `.env` - `PGADMIN_DEFAULT_EMAIL`)
- **Senha:** (conforme `.env` - `PGADMIN_DEFAULT_PASSWORD`)

### Configurar Conexão no PgAdmin

- **Host:** `postgres`
- **Port:** `5432`
- **Database:** (conforme `.env` - `POSTGRES_DB`)
- **Username:** (conforme `.env` - `POSTGRES_USER`)
- **Password:** (conforme `.env` - `POSTGRES_PASSWORD`)
