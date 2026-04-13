using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentWebSearchAggregator.Interfaces
{
    public interface ISearchProviderFactory
    {
        ISearchProvider Get(string source);
    }

}
