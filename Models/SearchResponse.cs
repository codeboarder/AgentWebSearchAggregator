using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentWebSearchAggregator.Models
{
    public sealed record SearchResponse(
        string Source,
        object Results
    );

}
