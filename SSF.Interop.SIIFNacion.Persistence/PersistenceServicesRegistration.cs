using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Persistence.DBContext;
using SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories;
using SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence;

namespace SSF.Interop.SIIFNacion.Persistence
{
    public static class PersistenceServicesRegistration
    {
        public static IServiceCollection ConfigurePresistenceServices(this IServiceCollection services,
            string auditoriaConnectionString,
            string procesoConnectionString)
        {
            services.AddDbContext<GestordocDbContext>(options =>
                options.UseSqlServer(procesoConnectionString));

            services.AddDbContext<DbContextCdp>(options =>
                options.UseSqlServer(procesoConnectionString));

            services.AddDbContext<CompromisoDBContext>(options =>
                options.UseSqlServer(procesoConnectionString));

            services.AddDbContext<CompromisoDynContext>(options =>
            {
                options.UseSqlServer(procesoConnectionString,
                    sqlOpts => sqlOpts.CommandTimeout(300)); // 5 min para operaciones masivas de RP
            });

            // Repositorios
            services.AddScoped<ICdpRepository, CdpRepository>();
            services.AddScoped<ICdpPaginadoRepository, CdpPaginadoRepository>();
            services.AddScoped<ICdpCompromisoRepository, CdpCompromisoRepository>();
            services.AddScoped<IDynCompromisoRepository, DynCompromisoRepository>();
            services.AddScoped<IObligacionApoRepository, ObligacionApoRepository>();
            services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
            services.AddScoped<IAuditoriaLogger, DynEfwAuditoriaLogger>();
            services.AddScoped<IEjecucionAgregadaRepository, EjecucionAgregadaRepository>();

            // ← NUEVO: lista paginada de compromisos RP → DYNTBLCOMPROMPAGIN
            services.AddScoped<IDynCompromPaginRepository, DynCompromPaginRepository>();

            return services;
        }
    }
}