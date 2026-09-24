using FCG.Application.Identidade.DTOs;
using FCG.Application.Shared;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Security;
using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Application.Identidade.UseCases;

public sealed class AlterarSenhaUseCase
{
    private readonly IInformacoesUsuarioLogado _informacoesUsuarioLogado;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;

    public AlterarSenhaUseCase
    (
        IInformacoesUsuarioLogado informacoesUsuarioLogado,
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher
    )
    {
        _informacoesUsuarioLogado = informacoesUsuarioLogado;
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
    }
    
    public async Task Executar(AlterarSenhaRequest request)
    {
        Usuario usuario = await _usuarioRepository.ObterPorIdTracking(_informacoesUsuarioLogado.Id)
            ?? throw new ValidationException("Usuário não encontrado");

        bool senhaValida = _senhaHasher.ValidarSenha(request.SenhaAtual, usuario.SenhaHash);
        
        if (!senhaValida)
            throw new ValidationException("Senha atual inválida");
        
        if (request.SenhaAtual == request.NovaSenha)
            throw new ValidationException("Nova senha deve ser diferente da senha atual");
        
        SenhaTextoPuro senhaTextoPuro = new(request.NovaSenha, request.ConfirmacaoNovaSenha);
        SenhaHash senhaHash = _senhaHasher.GerarHash(senhaTextoPuro);
        
        usuario.SetSenhaHash(senhaHash);
        
        await _usuarioRepository.UnitOfWork.Commit();
    }
}