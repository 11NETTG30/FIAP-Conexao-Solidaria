using ConexaoSolidaria.Application.Identidade.DTOs;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Domain.Identidade.Repositories;
using ConexaoSolidaria.Domain.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.ValueObjects;
using ConexaoSolidaria.Domain.Shared.Exceptions;

namespace ConexaoSolidaria.Application.Identidade.UseCases;

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
        Cpf cpf = new(request.Cpf);

        SenhaTextoPuro senhaTextoPuro = new(request.Senha, request.ConfirmacaoSenha);
        SenhaHash senhaHash = _senhaHasher.GerarHash(senhaTextoPuro);

        Usuario usuario = new(request.Nome, email, cpf, senhaHash, PerfilUsuario.Doador);

        bool emailExiste = await _usuarioRepository.VerificarExistenciaEmail(usuario.Email.Valor);

        if (emailExiste)
            throw new ConflictException("Já existe um usuário cadastrado com esse e-mail");

        bool cpfExiste = await _usuarioRepository.VerificarExistenciaCpf(usuario.Cpf.Valor);

        if (cpfExiste)
            throw new ConflictException("Já existe um usuário cadastrado com esse CPF");

        await _usuarioRepository.Adicionar(usuario);
        await _usuarioRepository.UnitOfWork.Commit();
        
        return usuario.Id;
    }
}