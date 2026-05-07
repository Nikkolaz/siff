using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Storage;
using SSF.Interop.SIIFNacion.Infrastructure.Network;
using SSF.Interop.SIIFNacion.Infrastructure.Options;
using SSF.Interop.SIIFNacion.Infrastructure.Services.External.Handlers;
using SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF;
using SSF.Interop.SIIFNacion.Infrastructure.Storage;
using SSF.Interop.SIIFNacion.Service.Infrastructure.Logging;

namespace SSF.Interop.SIIFNacion.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {


            services.Configure<SiifOptions>(configuration.GetSection(SiifOptions.SectionName));
            services.AddMemoryCache();

            services.AddTransient<SiifAuthDelegatingHandler>();
            services.AddTransient<SiifRetryOnUnauthorizedHandler>();

            services.AddScoped<ISiifTokenProvider, SiifTokenProvider>();
            services.AddScoped<ISiifAuthenticationService, SiifAuthenticationService>();
            services.AddScoped<ISiifBudgetService, SiifBudgetService>();
            services.AddScoped<ISiifCatalogService, SiifCatalogService>();

            services.AddHttpClient<ISiifAuthenticationService, SiifAuthenticationService>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SiifOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            services.AddHttpClient<ISiifBudgetService, SiifBudgetService>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SiifOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<SiifAuthDelegatingHandler>()
            .AddHttpMessageHandler<SiifRetryOnUnauthorizedHandler>();

            services.AddHttpClient<ISiifCatalogService, SiifCatalogService>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SiifOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<SiifAuthDelegatingHandler>()
            .AddHttpMessageHandler<SiifRetryOnUnauthorizedHandler>();

            


            services.AddScoped<EventViewerLogger>();
            services.AddScoped<DatabaseLogger>();
            services.AddScoped<ICustomLogger>(sp =>
            {
                var loggers = new List<ICustomLogger>
            {
                sp.GetRequiredService<EventViewerLogger>(),
                sp.GetRequiredService<DatabaseLogger>()
            };
                return new CompositeLogger(loggers);
            });
            services.AddTransient<IWebClientWrapper, WebClientWrapper>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            return services;
        }
    }
}
