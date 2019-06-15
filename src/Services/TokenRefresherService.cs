using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Api.Abstractions.Errors;
using Firepuma.MicroServices.Auth.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable PossibleInvalidOperationException

namespace Firepuma.MicroServices.Auth.Services
{
    internal class TokenRefresherService : BackgroundService
    {
        private readonly TokenRefreshOptions _options;
        private readonly ILogger<TokenRefresherService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IErrorReportingService _errorReportingService;

        public TokenRefresherService(
            IOptions<TokenRefreshOptions> options,
            ILogger<TokenRefresherService> logger,
            IServiceProvider serviceProvider,
            IErrorReportingService errorReportingService)
        {
            _options = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _errorReportingService = errorReportingService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("TokenRefresherService is starting.");

            cancellationToken.Register(() => _logger.LogDebug("TokenRefresherService background task is stopping due to cancellation."));

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("TokenRefresherService task doing background work.");

                await RefreshToken();

                await Task.Delay(_options.Interval.Value, cancellationToken);
            }

            _logger.LogDebug("TokenRefresherService background task is stopping.");
        }

        private async Task RefreshToken()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                if (!(scope.ServiceProvider.GetRequiredService<IMicroServiceTokenProvider>() is CachedMicroServiceTokenProvider cachedMicroServiceTokenProvider))
                {
                    _errorReportingService.CaptureException(new Exception("Critical error: Expected IMicroServiceTokenProvider to be of type CachedMicroServiceTokenProvider in TokenRefresherService"));

                    return;
                }

                await cachedMicroServiceTokenProvider.GetToken();
            }
        }
    }
}