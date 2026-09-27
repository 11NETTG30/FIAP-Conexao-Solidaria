using ConexaoSolidaria.Application.Doacoes.Services;
using ConexaoSolidaria.Domain.Campanhas.Enums;
using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Events;
using ConexaoSolidaria.Infrastructure.Campanhas.Persistence;
using ConexaoSolidaria.Infrastructure.Doacoes.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConexaoSolidaria.Worker.Consumers;

public sealed class DoacaoRecebidaEventConsumer : IConsumer<DoacaoRecebidaEvent>
{
    private readonly CampanhaDbContext _campanhaDbContext;
    private readonly DoacaoDbContext _doacaoDbContext;
    private readonly ILogger<DoacaoRecebidaEventConsumer> _logger;

    public DoacaoRecebidaEventConsumer
    (
        CampanhaDbContext campanhaDbContext,
        DoacaoDbContext doacaoDbContext,
        ILogger<DoacaoRecebidaEventConsumer> logger
    )
    {
        _campanhaDbContext = campanhaDbContext;
        _doacaoDbContext = doacaoDbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DoacaoRecebidaEvent> context)
    {
        DoacaoRecebidaEvent evento = context.Message;
        CancellationToken ct = context.CancellationToken;

        await using IDbContextTransaction transaction = await _doacaoDbContext.Database.BeginTransactionAsync(ct);
        await _campanhaDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);

        bool jaProcessada = await _doacaoDbContext.DoacoesProcessadas
            .FindAsync([evento.IdDoacao], ct) is not null;

        if (jaProcessada)
        {
            _logger.LogInformation(
                "Doação {IdDoacao} já processada — mensagem ignorada (idempotência)", evento.IdDoacao);
            await transaction.RollbackAsync(ct);
            return;
        }

        Doacao? doacao = await _doacaoDbContext.Doacoes.FindAsync([evento.IdDoacao], ct);

        if (doacao is null)
        {
            _logger.LogWarning("Doação {IdDoacao} não encontrada — mensagem descartada", evento.IdDoacao);
            await transaction.RollbackAsync(ct);
            return;
        }

        DateTime agora = DateTime.UtcNow;

        int linhasAfetadas = await _campanhaDbContext.Campanhas
            .Where(c => c.Id == evento.IdCampanha
                && c.Status == StatusCampanha.Ativa
                && c.DataFim >= agora)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.ValorArrecadado, c => c.ValorArrecadado + evento.ValorDoacao)
                .SetProperty(c => c.DataAtualizacao, _ => agora), ct);

        DecisaoProcessamentoDoacao.ResultadoProcessamentoDoacao resultado = DecisaoProcessamentoDoacao.Decidir(
            doacaoJaProcessada: false,
            campanhaAptaAReceberDoacao: linhasAfetadas == 1);

        if (resultado == DecisaoProcessamentoDoacao.ResultadoProcessamentoDoacao.Confirmada)
            doacao.Confirmar();
        else
            doacao.Rejeitar();

        _doacaoDbContext.DoacoesProcessadas.Add(new DoacaoProcessada(evento.IdDoacao));

        await _doacaoDbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        _logger.LogInformation("Doação {IdDoacao} processada como {Resultado}", evento.IdDoacao, resultado);
    }
}
