
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:7099/")
};

var requestBody = new
{
    source = "pubmed",
    query = "latest hypertension treatment guidelines",
    maxResults = 5
};

HttpResponseMessage response =
    await httpClient.PostAsJsonAsync("api/WebSearch", requestBody);

if (!response.IsSuccessStatusCode)
{
    var error = await response.Content.ReadAsStringAsync();
    throw new Exception(
        $"Egress broker call failed: {(int)response.StatusCode} - {error}");
}

var responseJson = await response.Content.ReadAsStringAsync();
Console.WriteLine(responseJson);
