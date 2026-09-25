using DotNetEnv;
using FCG.API.Configurations;
using FCG.API.Middlewares;
using FCG.Infrastructure.Campanhas.Persistence;
using FCG.Infrastructure.Configurations;
using FCG.Infrastructure.Identidade.Configurations;
using FCG.Infrastructure.Identidade.Persistence;
using FCG.IoC;
using Microsoft.AspNetCore.HttpOverrides;
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
builder.Services.AddDependencies();

WebApplication app = builder.Build();

// Render (e hosts similares) terminam o TLS na borda e repassam a requisição
// como HTTP puro pro container — sem isso, Request.Scheme fica "http" mesmo
// em produção e vaza pra geração de URL do Swagger/OpenAPI, quebrando o
// "Try it out" por mixed content/CORS de esquema.
ForwardedHeadersOptions forwardedHeadersOptions = new()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Hosts sem um passo de deploy separado para migrations (ex.: Render, Railway)
// aplicam as migrations pendentes aqui, atrás de uma flag — em dev local e no
// docker-compose isso é feito pelo serviço "migrate" antes da API subir.
if (builder.Configuration.GetValue<bool>("RUN_MIGRATIONS_ON_STARTUP"))
{
    using IServiceScope migrationScope = app.Services.CreateScope();
    migrationScope.ServiceProvider.GetRequiredService<IdentidadeDbContext>().Database.Migrate();
    migrationScope.ServiceProvider.GetRequiredService<CampanhaDbContext>().Database.Migrate();
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