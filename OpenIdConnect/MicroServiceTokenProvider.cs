using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Firepuma.MicroServices.Auth.OpenIdConnect
{
    // ReSharper disable once ClassNeverInstantiated.Global

    public class MicroServiceTokenProvider : IMicroServiceTokenProvider
    {
        private readonly IOptions<OpenIdConnectOptions> _options;
        private readonly HttpClient _client;

        public MicroServiceTokenProvider(IOptions<OpenIdConnectOptions> options)
        {
            _options = options;

            _client = new HttpClient();
        }

        public async Task<TokenResponse> GetToken()
        {
            var tokenEndpoint = await GetTokenEndpoint();

            const string grantType = "client_credentials";
            var form = new Dictionary<string, string>
            {
                {
                    "grant_type", grantType
                },
                {
                    "client_id", _options.Value.ClientId
                },
                {
                    "client_secret", _options.Value.ClientSecret
                },
            };

            var response = await _client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                var content = response.Content != null ? await response.Content.ReadAsStringAsync() : "";
                throw new Exception($"Status code {response.StatusCode} is not successful. Content: {content}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonContent);
            return tokenResponse;
        }

        private async Task<string> GetTokenEndpoint()
        {
            var jsonContent = await _client.GetStringAsync(_options.Value.Authority.TrimEnd('/') + "/.well-known/openid-configuration");
            var response = JsonConvert.DeserializeObject<OpenIdConnectInfo>(jsonContent);
            return response.TokenEndpoint;
        }

        private class OpenIdConnectInfo
        {
            [JsonProperty("token_endpoint")]
            public string TokenEndpoint { get; set; }
        }
    }
}
