using AgentWebSearchAggregator.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace AgentWebSearchAggregator.SearchProvider
{

    public class PubMedSearchProvider : ISearchProvider
    {
        private readonly HttpClient _http;

        public PubMedSearchProvider(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<object>> SearchAsync(
            string query,
            int maxResults)
        {
            var url =
                $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi" +
                $"?db=pubmed&term={Uri.EscapeDataString(query)}" +
                $"&retmode=json&retmax={maxResults}";

            var response = await _http.GetFromJsonAsync<JsonElement>(url);

            var ids = response
                .GetProperty("esearchresult")
                .GetProperty("idlist")
                .EnumerateArray()
                .Select(e => e.GetString()!)
                .ToList();

            return ids;
        }
    }
}
