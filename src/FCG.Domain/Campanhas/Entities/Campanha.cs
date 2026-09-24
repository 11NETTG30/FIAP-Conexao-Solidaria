using FCG.Domain.Campanhas.Enums;
using FCG.Domain.Shared.Abstractions;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Domain.Campanhas.Entities;

public sealed class Campanha : Entity, IAggregateRoot, IAuditavel
{
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public decimal MetaFinanceira { get; private set; }
    public StatusCampanha Status { get; private set; }

    // Só leitura para este módulo: quem incrementa é o worker de doações,
    // direto no schema "campanha", a partir do DoacaoRecebidaEvent.
    public decimal ValorArrecadado { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public Campanha
    (
        string titulo,
        string descricao,
        DateTime dataInicio,
        DateTime dataFim,
        decimal metaFinanceira
    )
    {
        SetTitulo(titulo);
        SetDescricao(descricao);
        SetPeriodo(dataInicio, dataFim);
        SetMetaFinanceira(metaFinanceira);
        SetStatus(StatusCampanha.Ativa);

        if (DataFim < DateTime.UtcNow)
            throw new ValidationException("Data de término da campanha não pode estar no passado");

        ValorArrecadado = 0;
    }

    // EF Core
    private Campanha(){}

    public void SetTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ValidationException("Título não pode ser vazio ou nulo");

        titulo = titulo.Trim();

        if (titulo.Length is < 3 or > 150)
            throw new ValidationException("Título deve ter entre 3 e 150 caracteres");

        Titulo = titulo;
    }

    public void SetDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ValidationException("Descrição não pode ser vazia ou nula");

        descricao = descricao.Trim();

        if (descricao.Length > 2000)
            throw new ValidationException("Descrição deve ter no máximo 2000 caracteres");

        Descricao = descricao;
    }

    public void SetPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        dataInicio = ParaUtc(dataInicio);
        dataFim = ParaUtc(dataFim);

        if (dataFim <= dataInicio)
            throw new ValidationException("Data de término deve ser posterior à data de início");

        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public void SetMetaFinanceira(decimal metaFinanceira)
    {
        if (metaFinanceira <= 0)
            throw new ValidationException("Meta financeira deve ser maior que zero");

        MetaFinanceira = metaFinanceira;
    }

    public void SetStatus(StatusCampanha status)
    {
        if (!Enum.IsDefined(status))
            throw new ValidationException("Status da campanha é inválido, valor não definido");

        Status = status;
    }

    // O Postgres (timestamp with time zone) só aceita DateTime em UTC — datas
    // que chegam sem fuso no JSON são tratadas como UTC.
    private static DateTime ParaUtc(DateTime data) =>
        data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };

    public override string ToString() =>
        $"{Titulo} - {Status} - {Id}";
}
