using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FCG.Infrastructure.Identidade.Configurations;

public static class AuthenticationConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddJwtAuthentication(IConfiguration configuration)
        {
            IConfigurationSection jwtSettings = configuration.GetSection("JwtSettings");
            
            services.AddOptions<JwtSettings>()
                .Bind(jwtSettings)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            string? secret = jwtSettings[nameof(JwtSettings.Secret)];
            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(secret!));

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings[nameof(JwtSettings.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtSettings[nameof(JwtSettings.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                
                ClockSkew = TimeSpan.Zero
            };
            
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            services.AddAuthorization();
        }
    }
}