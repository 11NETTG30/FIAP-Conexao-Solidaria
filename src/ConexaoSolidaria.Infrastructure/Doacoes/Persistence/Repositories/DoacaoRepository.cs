using ConexaoSolidaria.Domain.Doacoes.Entities;
using ConexaoSolidaria.Domain.Doacoes.Repositories;
using ConexaoSolidaria.Domain.Shared.UoW;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Repositories;

public sealed class DoacaoRepository : IDoacaoRepository
{
    private readonly DoacaoDbContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public DoacaoRepository(DoacaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Adicionar(Doacao doacao)
    {
        await _dbContext.Doacoes
            .AddAsync(doacao);
    }

    public async Task<Doacao?> ObterPorId(Guid id)
    {
        return await _dbContext.Doacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<ResultadoPaginado<Doacao>> Listar(FiltroListagemDoacao filtro)
    {
        IQueryable<Doacao> query = _dbContext.Doacoes.AsNoTracking();

        if (filtro.Status is not null)
            query = query.Where(d => d.Status == filtro.Status);

        if (filtro.IdCampanha is not null)
            query = query.Where(d => d.IdCampanha == filtro.IdCampanha);

        if (filtro.IdDoador is not null)
            query = query.Where(d => d.IdDoador == filtro.IdDoador);

        if (filtro.DataInicio is not null)
            query = query.Where(d => d.DataCriacao >= filtro.DataInicio);

        if (filtro.DataFim is not null)
            query = query.Where(d => d.DataCriacao <= filtro.DataFim);

        int totalItens = await query.CountAsync();

        List<Doacao> itens = await query
            .OrderByDescending(d => d.DataCriacao)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<Doacao>(itens, totalItens);
    }
    
    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}