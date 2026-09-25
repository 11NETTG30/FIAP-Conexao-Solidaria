using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConexaoSolidaria.Application.Identidade.Security;
using ConexaoSolidaria.Domain.Identidade.Entities;
using ConexaoSolidaria.Domain.Identidade.Enums;
using ConexaoSolidaria.Infrastructure.Identidade.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConexaoSolidaria.Infrastructure.Identidade.Security
{
    public sealed class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
    
        public JwtService
        (
            IOptions<JwtSettings> jwtSettings
        )
        {
            _jwtSettings = jwtSettings.Value;
        }
    
        public string GerarAccessToken(Usuario usuario)
        {
            List<Claim> claims = ObterClaims(usuario);

            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(_jwtSettings.Secret));
            SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore =  DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiracaoAccessTokenMinutos),
                SigningCredentials = credentials
            };
        
            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken? token = tokenHandler.CreateToken(securityTokenDescriptor);
            string? encodedToken = tokenHandler.WriteToken(token);

            return encodedToken;
        }

        private static List<Claim> ObterClaims(Usuario usuario)
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email.Valor),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ];

            string role = usuario.Perfil == PerfilUsuario.GestorONG
                ? RoleNames.GestorONG
                : RoleNames.Doador;

            claims.Add(new Claim(ClaimTypes.Role, role));
        
            return claims;
        }

    }
}