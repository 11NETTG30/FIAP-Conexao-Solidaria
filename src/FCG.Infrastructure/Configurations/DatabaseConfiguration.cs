using FCG.Infrastructure.Identidade.Persistence;
using FCG.Infrastructure.Shared.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Infrastructure.Configurations;

public static class DatabaseConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddDatabase(IConfiguration configuration)
        {
            string connectionString = ResolveConnectionString(configuration);

            services.AddSingleton<AuditoriaSaveChangesInterceptor>();
            services.AddDatabasePostgreSQL<IdentidadeDbContext>(connectionString, IdentidadeDbContext.SCHEMA);
        }

        private void AddDatabasePostgreSQL<T>(string connectionString, string schema) where T : DbContext
        {
            services.AddDbContext<T>((serviceProvider, options) =>
                options
                    .UseNpgsql(connectionString, optionsPostgress =>
                    {
                        optionsPostgress.MigrationsHistoryTable("__ef_migrations_history", schema);
                    })
                    .AddInterceptors(serviceProvider.GetRequiredService<AuditoriaSaveChangesInterceptor>())
            );
        }
    }

    // Alguns hosts (ex.: Render) não deixam compor uma connection string única no
    // provisionamento — só expõem host/porta/usuário/senha como variáveis soltas,
    // seguindo a convenção padrão do libpq (PGHOST, PGPORT, PGDATABASE, PGUSER,
    // PGPASSWORD). Quando PGHOST existir, monta a connection string a partir delas;
    // senão, usa ConnectionStrings:DefaultConnection normalmente (dev local, compose).
    private static string ResolveConnectionString(IConfiguration configuration)
    {
        string? pgHost = configuration["PGHOST"];

        if (string.IsNullOrWhiteSpace(pgHost))
            return configuration.GetConnectionString("DefaultConnection")!;

        string pgPort = configuration["PGPORT"] ?? "5432";
        string pgDatabase = configuration["PGDATABASE"] ?? "";
        string pgUser = configuration["PGUSER"] ?? "";
        string pgPassword = configuration["PGPASSWORD"] ?? "";

        return $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword}";
    }
}