using System;
using Firepuma.Api.Common.Configure;
using Firepuma.MicroServices.Auth.OpenIdConnect;
using Firepuma.MicroServices.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Firepuma.MicroServices.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenIdConnectTokenProvider(this IServiceCollection services,
            IConfigurationSection tokenProviderConfigSection)
        {
            services.ConfigureAndValidate<OpenIdConnectOptions>(tokenProviderConfigSection.Bind);

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var options = scope.ServiceProvider.GetRequiredService<IOptions<OpenIdConnectOptions>>().Value;

                if (options.AutoRefreshEnabled != true)
                {
                    RegisterWithoutAutoRefresh(services);
                }
                else
                {
                    if (options.AutoRefreshInterval == null)
                    {
                        throw new ArgumentOutOfRangeException("OpenIdConnectOptions.AutoRefreshInterval must be specified when AutoRefreshEnabled is true");
                    }

                    RegisterWithAutoRefresh(services, options.AutoRefreshInterval);
                }
            }
        }

        private static void RegisterWithoutAutoRefresh(IServiceCollection services)
        {
            services.AddScoped<IMicroServiceTokenProvider, MicroServiceTokenProvider>();
        }

        private static void RegisterWithAutoRefresh(IServiceCollection services, TimeSpan? optionsAutoRefreshInterval)
        {
            services.AddSingleton<IMicroServiceTokenProvider, CachedMicroServiceTokenProvider>(s =>
                new CachedMicroServiceTokenProvider(
                    new MicroServiceTokenProvider(s.GetRequiredService<IOptions<OpenIdConnectOptions>>()),
                    s.GetRequiredService<ILogger<CachedMicroServiceTokenProvider>>()));

            services.Configure<TokenRefreshOptions>(o => { o.Interval = optionsAutoRefreshInterval; });
            services.AddHostedService<TokenRefresherService>();
        }
    }
}