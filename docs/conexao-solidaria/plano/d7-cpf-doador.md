# D7 — CPF do doador (Cadastro de Doador)

Responsável: Gabriel. Não depende de nenhuma demanda em aberto — escopo
isolado dentro do módulo Identidade (D0, já fechada e mergeada).

## Objetivo

Adicionar o campo CPF (com validação de formato) ao cadastro de doador —
requisito funcional explícito do edital que ficou de fora do escopo herdado
do `fiap-cloud-games` e não foi mapeado em nenhuma demanda anterior (d0–d6).

## Como foi encontrado

Achado numa auditoria comparando o edital
(`../../../edital/HACKATHON_11NETT.pdf`, item 3 "Cadastro de Doador") com as
demandas d0–d6: o edital exige Nome Completo, Email (único), **CPF (validar
formato)** e Senha (hash). A entidade `Usuario` — herdada do boilerplate
`fiap-cloud-games` (plataforma de jogos, nunca teve esse campo) — só tem
`Nome`, `Email` e `SenhaHash`. Nenhuma das demandas d0–d6 menciona CPF.

## Contexto

- Campos hoje: `src/ConexaoSolidaria.Domain/Identidade/Entities/Usuario.cs`
  (Nome, Email, SenhaHash, Perfil, Ativo)
- Cadastro público: `POST /api/auth/registrar` (`AuthController` →
  `CriarUsuarioUseCase`) — já atribui `PerfilUsuario.Doador` por padrão, isso
  está correto e não precisa mudar
- Precisa de: novo Value Object `Cpf` (seguir o padrão de `Email`/`SenhaHash`
  já existentes em `Domain/Identidade/ValueObjects/`), com validação de
  formato de verdade (dígitos verificadores, não só "11 dígitos")

### Raio de impacto (tudo dentro do módulo Identidade)

- `Usuario.cs` (novo campo + validação)
- `CriarUsuarioRequest` (DTO) + `CriarUsuarioRequestValidator`
- `CriarUsuarioUseCase`
- `UsuarioConfiguration` (mapeamento EF) + nova migration no schema
  `identidade` — inclui ajustar o `HasData` do usuário admin de seed, que
  precisa ganhar um CPF fixo
- `e2e/smoke-test.sh`: todos os payloads de `POST /api/auth/registrar`
  (cenários A, R, S, T) precisam do campo novo, senão o script quebra
- Testes unitários em `tests/ConexaoSolidaria.Tests/Identidade/`
  (`UsuarioTests`, `CriarUsuarioUseCaseTests`, validator)

## Análise de risco / janela de tempo

Levantamento feito a pedido do Gabriel, para saber se dá para deixar essa
demanda por último (depois de D2–D5), sem atrapalhar quem estiver nelas.

- **Conflito de código com as demandas dos colegas: baixo/nenhum.** D0 —
  única demanda que mexeu no módulo Identidade — já está fechada e mergeada.
  D1 (Campanha) e D2 (Doação/Worker) referenciam usuário só pelo `Guid`
  (`IdDoador`), nunca instanciam nem leem campos de `Usuario` — confirmado
  por busca no repositório inteiro (`new Usuario(` só aparece dentro do
  próprio módulo, em `UsuarioTests.cs`). D3/D4/D5 não tocam em código de
  aplicação da Identidade. **Dá para fazer a qualquer momento, inclusive em
  paralelo com D2–D5, sem gerar conflito de merge.**
- **Migration de banco: sem conflito.** Cada módulo tem schema/DbContext
  próprio (`identidade`, `campanha`, `doacao`) — uma migration nova em
  `identidade` não toca nas migrations que D1/D2 já criaram em `campanha`/
  `doacao`.
- **O que de fato limita o prazo não são os colegas, é a D6**:
  - `e2e/smoke-test.sh` é o script de validação da API de Identidade (o
    `CLAUDE.md` exige rodá-lo antes de qualquer PR que mexa nela) — se CPF
    virar campo obrigatório, o script quebra até ser atualizado junto.
  - Se o README, prints do Swagger ou qualquer coleção Postman/Insomnia
    usada no vídeo forem preparados **antes** dessa mudança, ficam
    desatualizados e precisam ser refeitos.
  - O roteiro obrigatório do vídeo (edital, item 3 dos entregáveis) **não**
    exige mostrar o cadastro de doador — só autenticação/token, criação de
    campanha e simulação de doação — então isso não trava a gravação em si,
    só a precisão da documentação escrita.
- **Conclusão**: pode ficar para o fim da fila, mas precisa estar
  **concluída antes de a D6 travar o README e gravar o vídeo** — não antes
  de D2/D3/D4/D5, que não dependem disso em nada.

## Andamento

- [ ] Value Object `Cpf` (`Domain/Identidade/ValueObjects`) com validação de
      formato (dígitos verificadores)
- [ ] Adicionar `Cpf` à entidade `Usuario`
- [ ] Atualizar `CriarUsuarioRequest` + validator + `CriarUsuarioUseCase`
- [ ] Atualizar `UsuarioConfiguration` (EF) + nova migration (schema
      `identidade`) + CPF do admin de seed
- [ ] Atualizar `e2e/smoke-test.sh` com o campo novo em todos os payloads de
      registro
- [ ] Testes unitários (nome de método começando com `Ao`, sem underline)
      para o Value Object e o use case

## Pendências / dúvidas

- Confirmar com o grupo se CPF duplicado deve ser bloqueado — o edital só
  menciona unicidade explícita para o Email. Registrar a decisão aqui
  quando definida.

## Arquivos de apoio

- `../../../edital/HACKATHON_11NETT.pdf` — edital completo, item 3
  ("Cadastro de Doador")
- `../CONTEXTO-TECNICO.md` — padrão de módulo / Value Objects existentes
  (`Email`, `SenhaHash`)
