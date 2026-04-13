using AgentWebSearchAggregator.Interfaces;
using AgentWebSearchAggregator.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AgentWebSearchAggregator;


public class WebSearch
{
    private readonly ILogger _logger;
    private readonly ISearchProviderFactory _providerFactory;

    public WebSearch(
        ILoggerFactory loggerFactory,
        ISearchProviderFactory providerFactory)
    {
        _logger = loggerFactory.CreateLogger<WebSearch>();
        _providerFactory = providerFactory;
    }

    [Function("WebSearch")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req)
    {
        var request = await JsonSerializer.DeserializeAsync<SearchRequest>(
            req.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (request is null)
            return req.CreateResponse(HttpStatusCode.BadRequest);

        try
        {
            RequestValidator.Validate(request);

            var provider = _providerFactory.Get(request.Source);
            var results = await provider.SearchAsync(
                request.Query,
                request.MaxResults);

            LogSearch(request, results.Count);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(
                new SearchResponse(request.Source, results));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Search rejected");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync(ex.Message);
            return response;
        }
    }

    private void LogSearch(SearchRequest request, int count)
    {
        _logger.LogInformation(
            "Search executed: Source={Source}, QueryHash={Hash}, Count={Count}",
            request.Source,
            request.Query.GetHashCode(),
            count);
    }
}
