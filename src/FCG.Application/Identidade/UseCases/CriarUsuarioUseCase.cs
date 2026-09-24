using FCG.Application.Identidade.DTOs;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Domain.Identidade.Repositories;
using FCG.Domain.Identidade.Security;
using FCG.Domain.Identidade.ValueObjects;
using FCG.Domain.Shared.Exceptions;

namespace FCG.Application.Identidade.UseCases;

public sealed class CriarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;

    public CriarUsuarioUseCase
    (
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher
    )
    {
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
    }

    public async Task<Guid> Executar(CriarUsuarioRequest request)
    {
        Email email = new(request.Email);

        SenhaTextoPuro senhaTextoPuro = new(request.Senha, request.ConfirmacaoSenha);
        SenhaHash senhaHash = _senhaHasher.GerarHash(senhaTextoPuro);

        Usuario usuario = new(request.Nome, email, senhaHash, PerfilUsuario.Usuario);

        bool emailExiste = await _usuarioRepository.VerificarExistenciaEmail(usuario.Email.Valor);
        
        if (emailExiste)
            throw new ConflictException("Já existe um usuário cadastrado com esse e-mail");

        await _usuarioRepository.Adicionar(usuario);
        await _usuarioRepository.UnitOfWork.Commit();
        
        return usuario.Id;
    }
}