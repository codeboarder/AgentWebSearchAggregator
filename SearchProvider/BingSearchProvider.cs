using AgentWebSearchAggregator.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace AgentWebSearchAggregator.SearchProvider
{

    public class BingSearchProvider : ISearchProvider
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public BingSearchProvider(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["BingApiKey"]!;
        }

        public async Task<IReadOnlyList<object>> SearchAsync(
            string query,
            int maxResults)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.bing.microsoft.com/v7.0/search" +
                $"?q={Uri.EscapeDataString(query)}&count={maxResults}");

            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var results = json
                .GetProperty("webPages")
                .GetProperty("value")
                .EnumerateArray()
                .Select(v => new
                {
                    Title = v.GetProperty("name").GetString(),
                    Snippet = v.GetProperty("snippet").GetString(),
                    Url = v.GetProperty("url").GetString()
                })
                .ToList<object>();

            return results;
        }
    }

}
