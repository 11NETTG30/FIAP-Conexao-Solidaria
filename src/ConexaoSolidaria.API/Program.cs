using DotNetEnv;
using ConexaoSolidaria.API.Configurations;
using ConexaoSolidaria.API.Middlewares;
using ConexaoSolidaria.Infrastructure.Campanhas.Persistence;
using ConexaoSolidaria.Infrastructure.Configurations;
using ConexaoSolidaria.Infrastructure.Doacoes.Persistence;
using ConexaoSolidaria.Infrastructure.Identidade.Configurations;
using ConexaoSolidaria.Infrastructure.Identidade.Persistence;
using ConexaoSolidaria.IoC;
using MassTransit;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.AddLoggingConfiguration();
builder.Services.AddControllersConfiguration();
builder.Services.AddDocumentation();
builder.Services.AddProblemDetailsConfiguration();
builder.Services.ConfigureModelStateInvalid();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMessaging(
    builder.Configuration,
    configurarOutbox: busConfigurator => busConfigurator.AddEntityFrameworkOutbox<DoacaoDbContext>(outbox =>
    {
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    }));
builder.Services.AddDependencies();

WebApplication app = builder.Build();

app.UseForwardedHeadersConfiguration();

// Hosts sem um passo de deploy separado para migrations (ex.: Render, Railway)
// aplicam as migrations pendentes aqui, atrás de uma flag — em dev local e no
// docker-compose isso é feito pelo serviço "migrate" antes da API subir.
if (builder.Configuration.GetValue<bool>("RUN_MIGRATIONS_ON_STARTUP"))
{
    using IServiceScope migrationScope = app.Services.CreateScope();
    migrationScope.ServiceProvider.GetRequiredService<IdentidadeDbContext>().Database.Migrate();
    migrationScope.ServiceProvider.GetRequiredService<CampanhaDbContext>().Database.Migrate();
    migrationScope.ServiceProvider.GetRequiredService<DoacaoDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDocumentation();
}

app.UseGlobalExceptionMiddleware();
app.UseUnauthorizedAccessExceptionMiddleware();
app.UseDomainExceptionMiddleware();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();