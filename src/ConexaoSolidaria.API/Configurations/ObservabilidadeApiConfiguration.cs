using ConexaoSolidaria.Infrastructure.Configurations;
using ConexaoSolidaria.Infrastructure.Identidade.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ConexaoSolidaria.API.Configurations;

public static class ObservabilidadeApiConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddObservabilidadeApi()
        {
            builder.AddObservabilidade(
                configurarMetricas: metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter(),
                configurarTracing: tracing => tracing
                    .AddAspNetCoreInstrumentation(o => o.Filter = contexto => !EhRotaDeObservabilidade(contexto.Request.Path)));

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<IdentidadeDbContext>("postgres");
        }
    }

    extension(WebApplication app)
    {
        public void MapObservabilidade()
        {
            app.MapPrometheusScrapingEndpoint("/metrics");

            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        }
    }

    private static bool EhRotaDeObservabilidade(PathString caminho) =>
        caminho.StartsWithSegments("/metrics") || caminho.StartsWithSegments("/health");
}
