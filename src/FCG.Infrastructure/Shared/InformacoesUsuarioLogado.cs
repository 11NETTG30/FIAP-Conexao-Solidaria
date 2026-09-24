using System.Security.Claims;
using FCG.Application.Shared;
using FCG.Domain.Shared.Exceptions;
using FCG.Infrastructure.Identidade.Security;
using Microsoft.AspNetCore.Http;

namespace FCG.Infrastructure.Shared;

public sealed class InformacoesUsuarioLogado : IInformacoesUsuarioLogado
{
    public Guid Id { get; }
    public string Email { get; }
    public bool Administrador { get; set; }

    
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
        Administrador = user.IsInRole(RoleNames.Admin);
    }
}