using AgentWebSearchAggregator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentWebSearchAggregator.SearchProvider
{

    public class SearchProviderFactory : ISearchProviderFactory
    {
        private readonly IServiceProvider _services;

        public SearchProviderFactory(IServiceProvider services)
        {
            _services = services;
        }

        public ISearchProvider Get(string source) =>
            source switch
            {
                "pubmed" => _services.GetRequiredService<PubMedSearchProvider>(),
                "bing" => _services.GetRequiredService<BingSearchProvider>(),
                _ => throw new InvalidOperationException("Unknown provider")
            };
    }

}
