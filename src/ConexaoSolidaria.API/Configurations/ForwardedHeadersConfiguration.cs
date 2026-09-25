using Microsoft.AspNetCore.HttpOverrides;

namespace ConexaoSolidaria.API.Configurations;

public static class ForwardedHeadersConfiguration
{
    extension(WebApplication app)
    {
        public void UseForwardedHeadersConfiguration()
        {
            ForwardedHeadersOptions options = new()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            // IP do proxy do Render não é previsível, então não dá pra restringir por rede confiável
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            app.UseForwardedHeaders(options);
        }
    }
}
