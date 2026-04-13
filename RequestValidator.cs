using AgentWebSearchAggregator.Models;
using System.Text.RegularExpressions;

namespace AgentWebSearchAggregator
{
    public static class RequestValidator
    {
        private static readonly HashSet<string> AllowedSources =
            ["pubmed", "bing"];

        private static readonly Regex PhiPattern =
            new(@"\b(MRN|DOB|\d{3}-\d{2}-\d{4})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void Validate(SearchRequest request)
        {
            if (!AllowedSources.Contains(request.Source))
                throw new ArgumentException("Unsupported source");

            if (string.IsNullOrWhiteSpace(request.Query))
                throw new ArgumentException("Query is required");

            if (request.Query.Length > 256)
                throw new ArgumentException("Query too long");

            if (PhiPattern.IsMatch(request.Query))
                throw new ArgumentException("Potential PHI detected");
        }
    }

}
