using AgentWebSearchAggregator.Interfaces;
using AgentWebSearchAggregator.SearchProvider;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();

        services.AddSingleton<ISearchProviderFactory, SearchProviderFactory>();
        services.AddTransient<PubMedSearchProvider>();
        services.AddTransient<BingSearchProvider>();
    })
    .Build();

host.Run();

