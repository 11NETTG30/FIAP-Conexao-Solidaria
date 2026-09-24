# D1 — Módulo Campanha

Responsável sugerido: **Saulo**. Depende de D0 já estar mergeado na branch
principal.

## Objetivo

Implementar o CRUD de campanhas e o painel público de transparência,
seguindo exatamente o padrão do módulo `Identidade` já existente.

## Instruções

Herda tudo de `visao-geral.md` e de
`../CONTEXTO-TECNICO.md` — **ler os dois antes de
começar**, especialmente a seção "Padrão de módulo — Identidade como
referência" do segundo. Não reinventar estrutura: espelhar
`src/FCG.Domain/Identidade`, `src/FCG.Application/Identidade`,
`src/FCG.Infrastructure/Identidade`.

## Contexto

Regras extraídas do edital (`../../../edital/HACKATHON_11NETT.pdf`):

- **Entidade Campanha**: `Titulo` (string), `Descricao` (string),
  `DataInicio` (datetime), `DataFim` (datetime), `MetaFinanceira` (decimal),
  `Status` (Ativa/Concluida/Cancelada), `ValorArrecadado` (decimal, inicia
  em 0)
- **Regra de negócio**: campanha não pode ser criada com `DataFim` no
  passado; `MetaFinanceira` deve ser maior que zero
- **`CampanhaDbContext`** com `SCHEMA = "campanha"` — seguir exatamente o
  padrão de `IdentidadeDbContext.cs`
- **Endpoints**:
  - `POST /campanhas` — só `GestorONG` — cria com `Status: Ativa`
  - `PUT /campanhas/{id}` — só `GestorONG` — edita
  - `GET /campanhas` — público, sem JWT — só `Status: Ativa`, retornando
    `Titulo`, `MetaFinanceira`, `ValorArrecadado` (não expor os demais
    campos a menos que o edital peça)
- **Não implementar** aqui a atualização do `ValorArrecadado` — isso é
  responsabilidade do `doacoes-worker` (demanda D2), que escreve direto no
  schema `campanha` numa transação própria. O módulo Campanha só expõe o
  campo para leitura.

Consulte `../ARQUITETURA.md`, seção "Agregados", para entender por
que Campanha não tem uma lista de Doações dentro dela.

## Decisões

(nenhuma específica desta demanda ainda — decisões de arquitetura ficam no
`visao-geral.md`)

## Andamento

- [ ] Entidade `Campanha` (Domain) com validações de `DataFim` e
      `MetaFinanceira`
- [ ] `CampanhaDbContext` + `IEntityTypeConfiguration<Campanha>`
- [ ] Repositório `ICampanhaRepository` / `CampanhaRepository`
- [ ] Casos de uso: `CriarCampanhaUseCase`, `EditarCampanhaUseCase`,
      `ListarCampanhasAtivasUseCase`
- [ ] `CampanhaController` com os 3 endpoints
- [ ] Registrar `CampanhaDbContext` em `DatabaseConfiguration.cs` e o
      repositório em `DependencyInjectionInfrastructure.cs`
- [ ] Testes unitários das regras de validação (nomenclatura `Ao...`, sem
      underline)
- [ ] Migration inicial do schema `campanha`

## Pendências / dúvidas

- Confirmar se o `GET /campanhas` deve paginar (edital não especifica
  volume esperado)

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo
- `../ARQUITETURA.md` — arquitetura geral
