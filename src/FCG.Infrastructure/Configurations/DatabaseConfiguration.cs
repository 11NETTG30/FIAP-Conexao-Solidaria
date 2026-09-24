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
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            
            services.AddSingleton<AuditoriaSaveChangesInterceptor>();
            services.AddDatabasePostgreSQL<IdentidadeDbContext>(connectionString!, IdentidadeDbContext.SCHEMA);
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
}