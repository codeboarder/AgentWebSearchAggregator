# Agent Web Search Aggregator

This project was created as a demo to show how a Microsoft Foundry agent can call a Web Search tool, while keeping the calls internally in a Bring-Your-Own-Vnet scenario. The delegated subnet for Microsoft Foundry is used for outbound traffic only for private tools.
When Web Search is enabled, the agent requires Microsoft-managed public egress, which does not traverse customer UDRs or firewalls. Web Search calls can be routed to a search aggregator that only allows certain public endpoints and can be fronted by the Azure API Management service. Two sample public search endpoints are included: one for pubmed and one for bing.

## Overview

`AgentWebSearchAggregator` is an HTTP-triggered Azure Functions app designed to act as a single egress/search broker for Foundry Agents. It provides:

- A single endpoint: `POST /api/WebSearch`
- Provider routing by `source` (`pubmed` or `bing`)
- Basic request validation (source, query, size, PHI pattern check)
- Provider-specific result retrieval and a common response envelope

This repository also includes a small console app (`FunctionCaller`) to quickly invoke the function during local development.

That this repo does not provide:

- Authentication code
- APIM implementation
- Azure infrastructure

## Repository Structure

```text
.
|- Program.cs                         # Functions host + DI registration
|- WebSearch.cs                       # HTTP function entry point
|- RequestValidator.cs                # Input validation rules
|- Models/
|  |- SearchRequest.cs                # Request contract
|  |- SearchResponse.cs               # Response contract
|- Interfaces/
|  |- ISearchProvider.cs              # Provider contract
|  |- ISearchProviderFactory.cs       # Provider factory contract
|- SearchProvider/
|  |- PubMedSearchProvider.cs         # PubMed implementation
|  |- BingSearchProvider.cs           # Bing implementation
|  |- SearchProviderFactory.cs        # source -> provider resolver
|- FunctionCaller/
|  |- Program.cs                      # Local caller sample
```

## How It Works

1. Client sends JSON payload to `POST /api/WebSearch`.
2. Function deserializes payload into `SearchRequest`.
3. `RequestValidator` enforces source/query constraints.
4. `SearchProviderFactory` resolves provider from `source`.
5. Provider executes remote search request.
6. Function returns `SearchResponse` with source + provider results.

## API Contract

### Endpoint

- Method: `POST`
- URL (local): `http://localhost:7099/api/WebSearch`
- Auth level: `Anonymous` (local/testing scenarios)

### Request Body

```json
{
  "source": "pubmed",
  "query": "latest hypertension treatment guidelines",
  "maxResults": 5
}
```

### Request Fields

- `source` (`string`, required): One of `pubmed`, `bing`
- `query` (`string`, required): Non-empty, maximum 256 chars
- `maxResults` (`int`, optional): Defaults to `5` if omitted

### Successful Response

HTTP `200 OK`

```json
{
  "source": "pubmed",
  "results": [
    "40598654",
    "40598077",
    "40594432"
  ]
}
```

### Error Responses

HTTP `400 Bad Request` for:

- Invalid/unsupported `source`
- Missing/empty query
- Query length > 256
- Query matching PHI pattern heuristic
- Malformed request payload

Error body is plain text with a short reason message in validation exceptions.

## Validation Rules

Current request checks:

- Allowed sources: `pubmed`, `bing`
- `query` must be present and non-whitespace
- `query` length must be <= `256`
- Basic PHI regex check blocks terms like:
  - `MRN`
  - `DOB`
  - SSN-like pattern (`###-##-####`)

## Providers

### PubMed Provider

- Uses NCBI E-utilities `esearch.fcgi`
- Returns PubMed ID list (`idlist`) as the result array

### Bing Provider

- Uses Bing Web Search API v7
- Requires `BingApiKey` configuration value
- Returns array of objects containing:
  - `title`
  - `snippet`
  - `url`

## Prerequisites

- .NET SDK 8.0+ (function project target: `net8.0`)
- Azure Functions Core Tools v4 (for local function host execution)
- Optional: .NET SDK 10.0 preview if you want to run the `FunctionCaller` project as-is (`net10.0`)
- Bing Search resource + key (only when using `source = bing`)

## Local Configuration

Create `local.settings.json` in the repository root (this file is gitignored):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "BingApiKey": "<your-bing-api-key>"
  }
}
```

Notes:

- `BingApiKey` is required only for Bing requests.
- If not using local storage emulator, provide a valid Azure Storage connection string.

## Build and Run

### Option 1: Run with Functions Core Tools (recommended)

```bash
dotnet restore
dotnet build
func start --port 7099
```

### Option 2: Run from project directly

```bash
dotnet run --project AgentWebSearchAggregator.csproj -- --port 7099
```

## Test the Endpoint

### cURL (PubMed)

```bash
curl -X POST "http://localhost:7099/api/WebSearch" \
  -H "Content-Type: application/json" \
  -d '{"source":"pubmed","query":"latest hypertension treatment guidelines","maxResults":5}'
```

### cURL (Bing)

```bash
curl -X POST "http://localhost:7099/api/WebSearch" \
  -H "Content-Type: application/json" \
  -d '{"source":"bing","query":"hypertension guidelines","maxResults":3}'
```

### Use Included Caller App

```bash
dotnet run --project FunctionCaller/FunctionCaller.csproj
```

By default, `FunctionCaller` posts to `http://localhost:7099/api/WebSearch` using `pubmed`.

## Dependency Injection and Extensibility

Providers are resolved through `ISearchProviderFactory` and registered in DI.

To add a new provider:

1. Implement `ISearchProvider`.
2. Register the provider type in `Program.cs`.
3. Update `SearchProviderFactory` to map new `source` value.
4. Add new source value to `RequestValidator.AllowedSources`.
5. Update this README API docs with the new provider behavior.

## Security and Operational Notes

- Endpoint is currently anonymous; restrict access in production.
- Validation includes only lightweight PHI heuristics and is not a full compliance control.
- Logging stores query hash, not raw query text.
- Provider failures bubble through HTTP exceptions and currently become `400` with message; production hardening may split client vs upstream error classes.

## Troubleshooting

### `Unsupported source`

- Ensure `source` is exactly `pubmed` or `bing` (lowercase).

### `Query is required` / `Query too long`

- Provide a non-empty query <= 256 characters.

### `Potential PHI detected`

- Remove PHI-like tokens (`MRN`, `DOB`, SSN format) from the query.

### Bing calls fail with auth/403

- Verify `BingApiKey` in local settings.
- Confirm the key has access to Bing Search v7 endpoint.

### Function fails on startup due to settings

- Verify `local.settings.json` exists and includes required `Values` entries.

## Technology Stack

- Azure Functions (dotnet isolated worker, v4)
- .NET 8 (`AgentWebSearchAggregator`)
- `HttpClient` for outbound provider calls
- System.Text.Json for serialization

## License

See `LICENSE.txt`.
