using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using SSF.Interop.SIIFNacion.Application;
using SSF.Interop.SIIFNacion.Infrastructure;
using SSF.Interop.SIIFNacion.Middleware;
using SSF.Interop.SIIFNacion.Persistence;

try
{

    var builder = WebApplication.CreateBuilder(args);

    // Configurar servicios
    ConfigureServices(builder);
    
    var conn = builder.Configuration["ConnectionStrings:SqlServerProceso:Connection"];
    if (!string.IsNullOrEmpty(conn))
    {
        var builderConn = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(conn);
        Console.WriteLine($"[INFO] Conectando a Servidor: {builderConn.DataSource}, Base de datos: {builderConn.InitialCatalog}");
    }

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // En contenedor solo suele exponerse HTTP (p. ej. puerto 80); la redirección HTTPS rompe probes y clientes.
    var runningInContainer = string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);
    if (!runningInContainer)
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();

    app.MapControllers();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHealthChecks("/healthcheck", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/healthcheckui";
    });

    app.Run();

}
catch (Exception ex)
{

    throw new Exception($"Error: {JsonConvert.SerializeObject(ex)}");
}

// M�todo para configurar servicios de la aplicaci�n.
void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    builder.Services.AddHealthChecks();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureApplicationServices();
    builder.Services.ConfigureInfrastructureServices(builder.Configuration);
    var sqlServerConnection = BuildSqlServerConnectionString(builder.Configuration);
    builder.Services.ConfigurePresistenceServices(sqlServerConnection.Item1, sqlServerConnection.Item2);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
}

static (string, string) BuildSqlServerConnectionString(IConfiguration configuration)
{
    return (/*configuration["ConnectionStrings:SqlServerAuditoria:Connection"],*/  configuration["ConnectionStrings:SqlServerProceso:Connection"],configuration["ConnectionStrings:SqlServerProceso:Connection"]);
}

