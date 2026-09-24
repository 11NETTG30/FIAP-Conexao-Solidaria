using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG.Application.Identidade.Security;
using FCG.Domain.Identidade.Entities;
using FCG.Domain.Identidade.Enums;
using FCG.Infrastructure.Identidade.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FCG.Infrastructure.Identidade.Security
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

            if (usuario.Perfil == PerfilUsuario.Administrador)
                claims.Add(new Claim(ClaimTypes.Role, RoleNames.Admin));
        
            return claims;
        }

    }
}