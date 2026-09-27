using ConexaoSolidaria.Infrastructure.Campanhas.Persistence;
using ConexaoSolidaria.Infrastructure.Configurations;
using ConexaoSolidaria.Infrastructure.Doacoes.Persistence;
using ConexaoSolidaria.Infrastructure.Shared.Persistence.Interceptors;
using ConexaoSolidaria.Worker.Consumers;
using DotNetEnv;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

string connectionString = DatabaseConfiguration.ResolveConnectionString(builder.Configuration);

builder.Services.AddSingleton<AuditoriaSaveChangesInterceptor>();

// CampanhaDbContext e DoacaoDbContext precisam compartilhar a mesma
// NpgsqlConnection (não só a mesma connection string) para o consumer poder
// abrir uma única transação Postgres cobrindo os dois schemas — por isso o
// worker não usa o AddDatabase() padrão da API (que dá uma conexão por
// contexto, do pool).
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));

builder.Services.AddDbContext<CampanhaDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(serviceProvider.GetRequiredService<NpgsqlConnection>(), npgsqlOptions =>
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", CampanhaDbContext.SCHEMA))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditoriaSaveChangesInterceptor>()));

builder.Services.AddDbContext<DoacaoDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(serviceProvider.GetRequiredService<NpgsqlConnection>(), npgsqlOptions =>
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", DoacaoDbContext.SCHEMA))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditoriaSaveChangesInterceptor>()));

builder.Services.AddMessaging(
    builder.Configuration,
    registrarConsumidores: busConfigurator => busConfigurator.AddConsumer<DoacaoRecebidaEventConsumer>(),
    configurarEndpoints: (context, rabbitConfigurator) =>
    {
        rabbitConfigurator.ReceiveEndpoint("doacao-recebida", endpoint =>
        {
            endpoint.UseMessageRetry(retry => retry.Intervals(
                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2)));
            endpoint.ConfigureConsumer<DoacaoRecebidaEventConsumer>(context);
        });
    });

IHost host = builder.Build();
host.Run();
