# FIAP Cloud Games (FCG)

## 📚 Sobre o Projeto

Este repositório faz parte do **Tech Challenge da Pós-Graduação em Arquitetura de Sistemas .NET da FIAP**, **Turma 11NETT – Grupo 30**.

O objetivo do projeto é a construção de uma **plataforma de games educacionais**, chamada **FIAP Cloud Games (FCG)**, voltada para o aprendizado e prática de conceitos de tecnologia, utilizando boas práticas de arquitetura de software.

---

## 🎯 Objetivos do Tech Challenge

Os principais objetivos deste projeto são:

- Desenvolver uma **API backend** para uma plataforma de games educacionais
- Implementar **casos de uso reais**, como cadastro e autenticação de usuários
- Utilizar **JWT** para autenticação e autorização
- Persistir dados utilizando **Entity Framework Core**
- Documentar a API com **Swagger**
- Implementar **testes unitários**

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
docker exec -it fcg-postgres psql -U fcgadmin -d fcg_plataforma_jogos

# Caso dê erro para subir o container postgres, rode o seguinte comando no terminal:
wsl dos2unix scripts/init-database.sh
```

### 4. Aplicar Migrations

No console do Gerenciador de Pacotes, selecione o projeto padrão (ex: `src\FCG.Infrastructure`) e execute os comandos:

```powershell
# Criar uma nova migration
Add-Migration InitialIdentidade -Context IdentidadeDbContext -OutputDir Identidade/Persistence/Migrations

# Aplicar as alterações no banco de dados
Update-Database
```

### 5. Execução inicial da Aplicação

```bash
# Para iniciar o banco de dados, no PowerShell
docker-compose up -d

# Aplicar as alterações no banco de dados, no Console do Gerenciador de Pacotes
Update-Database
```

Rodar a API (FCG.API)

Acesse: https://localhost:5001/swagger

## Dados do administrador para login:
```json
{
	"email": "admin@fcg.com.br",
	"senha": "Admin@123"
}
```

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