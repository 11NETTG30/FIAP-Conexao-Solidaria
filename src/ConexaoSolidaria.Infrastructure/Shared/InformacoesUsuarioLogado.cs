using System.Security.Claims;
using ConexaoSolidaria.Application.Shared;
using ConexaoSolidaria.Domain.Shared.Exceptions;
using ConexaoSolidaria.Infrastructure.Identidade.Security;
using Microsoft.AspNetCore.Http;

namespace ConexaoSolidaria.Infrastructure.Shared;

public sealed class InformacoesUsuarioLogado : IInformacoesUsuarioLogado
{
    public Guid Id { get; }
    public string Email { get; }
    public bool GestorONG { get; set; }

    public InformacoesUsuarioLogado
    (
        IHttpContextAccessor httpContextAccessor
    )
    {
        if (!(httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
            throw new ValidationException("Usuário não está autenticado");

        ClaimsPrincipal user = httpContextAccessor.HttpContext.User;

        Id = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Email = user.FindFirst(ClaimTypes.Email)!.Value;
        GestorONG = user.IsInRole(RoleNames.GestorONG);
    }
}