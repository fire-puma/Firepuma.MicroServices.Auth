using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Firepuma.MicroServices.Auth.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable UnusedMember.Global

namespace Firepuma.MicroServices.Auth
{
    public class MicroServiceClient
    {
        private readonly ILogger _logger;
        private readonly IMicroServiceTokenProvider _microServiceTokenProvider;
        private readonly HttpClient _httpClient;
        private readonly Uri _baseUri;

        public MicroServiceClient(ILogger logger, IMicroServiceTokenProvider microServiceTokenProvider, Uri baseUri)
        {
            _logger = logger;
            _microServiceTokenProvider = microServiceTokenProvider;
            _baseUri = baseUri;

            _httpClient = new HttpClient();
        }

        public void AddDefaultHeaders(Dictionary<string, string> headers)
        {
            foreach (var headerPair in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(headerPair.Key, headerPair.Value);
            }
        }

        private async Task<Uri> GetUriAndPrepClient(string relativeUrl)
        {
            if (!Uri.TryCreate(_baseUri, relativeUrl, out var uri))
            {
                throw new Exception($"Error parsing baseUrl with relativeUrl '{relativeUrl}'");
            }

            var token = await _microServiceTokenProvider.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return uri;
        }

        public async Task<TResponse> Get<TResponse>(string relativeUrl)
        {
            var uri = await GetUriAndPrepClient(relativeUrl);
            var response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                var content = response.Content != null ? await response.Content.ReadAsStringAsync() : "";

                _logger.LogError($"Status code {response.StatusCode} is not successful. Content: {content}");

                throw new MicroServiceHttpException(response.StatusCode, response.ReasonPhrase);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            return FromJson<TResponse>(responseString);
        }

        public async Task<TResponse> Post<TRequest, TResponse>(string relativeUrl, TRequest payload)
        {
            var uri = await GetUriAndPrepClient(relativeUrl);

            var stringPayload = await Task.Run(() => ToJson(payload));

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(uri, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var content = response.Content != null ? await response.Content.ReadAsStringAsync() : "";

                _logger.LogError($"Status code {response.StatusCode} is not successful. Content: {content}");

                throw new MicroServiceHttpException(response.StatusCode, response.ReasonPhrase);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            return FromJson<TResponse>(responseString);
        }

        private static JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
            };
        }

        private static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
        }

        private static async Task<string> ToJson<T>(T payload)
        {
            return await Task.Run(() => JsonConvert.SerializeObject(payload, GetJsonSettings()));
        }
    }
}