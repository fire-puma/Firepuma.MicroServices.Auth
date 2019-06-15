using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Firepuma.MicroServices.Auth.OpenIdConnect
{
    public class CachedMicroServiceTokenProvider : IMicroServiceTokenProvider
    {
        private readonly IMicroServiceTokenProvider _provider;
        private readonly ILogger<CachedMicroServiceTokenProvider> _logger;

        private TokenResponse _token;
        private DateTime? _tokenExpiry;

        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1);

        public CachedMicroServiceTokenProvider(
            IMicroServiceTokenProvider provider,
            ILogger<CachedMicroServiceTokenProvider> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task<TokenResponse> GetToken()
        {
            if (_tokenExpiry != null && _tokenExpiry.Value > DateTime.UtcNow.AddMinutes(1))
            {
                return _token;
            }

            await _refreshLock.WaitAsync();

            try
            {
                if (_tokenExpiry != null && _tokenExpiry.Value > DateTime.UtcNow.AddMinutes(1))
                {
                    return _token;
                }

                _logger.LogDebug("Refreshing token");

                _token = await _provider.GetToken();
                _tokenExpiry = DateTime.UtcNow.AddSeconds(_token.ExpiresIn);
            }
            finally
            {
                _refreshLock.Release();
            }

            return _token;
        }
    }
}