using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ConexaoSolidaria.Infrastructure.Configurations;

public static class ObservabilidadeConfiguration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddObservabilidade
        (
            Action<MeterProviderBuilder>? configurarMetricas = null,
            Action<TracerProviderBuilder>? configurarTracing = null
        )
        {
            string nomeServico = builder.Configuration["OTEL_SERVICE_NAME"] is { Length: > 0 } nome
                ? nome
                : builder.Environment.ApplicationName;
            bool exportarOtlp = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            OpenTelemetryBuilder openTelemetry = builder.Services.AddOpenTelemetry()
                .ConfigureResource(recurso => recurso.AddService(nomeServico))
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddHttpClientInstrumentation()
                        .AddMeter("System.Runtime")
                        .AddMeter(InstrumentationOptions.MeterName);

                    configurarMetricas?.Invoke(metrics);

                    if (exportarOtlp)
                        metrics.AddOtlpExporter();
                });

            if (!exportarOtlp)
                return;

            openTelemetry
                .WithTracing(tracing =>
                {
                    tracing
                        .AddHttpClientInstrumentation()
                        .AddSource(DiagnosticHeaders.DefaultListenerName);

                    configurarTracing?.Invoke(tracing);

                    tracing.AddOtlpExporter();
                })
                .WithLogging(
                    logs => logs.AddOtlpExporter(),
                    options =>
                    {
                        options.IncludeFormattedMessage = true;
                        options.IncludeScopes = true;
                    });
        }
    }
}
