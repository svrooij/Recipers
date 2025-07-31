using System.Net;
using Recipers.Api.Tests.Helpers;
using FluentAssertions;
namespace Recipers.Api.Tests;

[Collection(MinimalApiEndpointsValidator.CollectionName)]
public class MinimalApiEndpointsValidator
{
    internal const string CollectionName = "MinimalApiEndpoints";
    private readonly RecipeApiFactory _factory;
    private readonly HttpClient _client;

    public MinimalApiEndpointsValidator(RecipeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Recipes_endpoint_should_return_statuscode_401_without_token()
    {
        // Test that endpoints require authorization
        var response = await _client.GetAsync("/recipes", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WeatherForecast_endpoint_should_return_statuscode_401_without_token()
    {
        // Test that endpoints require authorization
        var response = await _client.GetAsync("/weatherforecast", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Retrieves all endpoints from the OpenAPI documentation.
    /// This method fetches the OpenAPI JSON document, parses it, and yields each endpoint
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> GetAllEndpointsUsingOpenApiSpec()
    {
        // Because this is a static method, we need to create a new factory instance
        var factory = new RecipeApiFactory();
        var client = factory.CreateClient();

        // This cannot be an async method because it is used in a MemberData attribute, and xunit does not support async methods in MemberData
        var response = client.GetAsync("/openapi/v1.json").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var paths = doc.RootElement.GetProperty("paths");
        foreach (var pathProp in paths.EnumerateObject())
        {
            foreach (var methodProp in pathProp.Value.EnumerateObject())
            {
                yield return new object[] { pathProp.Name, methodProp.Name };
            }
        }
    }

    // ------------------------- Authentication Tests -------------------------
    [Theory]
    [MemberData(nameof(GetAllEndpointsUsingOpenApiSpec))]
    public async Task Endpoint_should_return_statuscode_401_without_valid_token(string path, string method)
    {
        var req = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), path.Replace("{id}", Guid.NewGuid().ToString()));
        var result = await _client.SendAsync(req, TestContext.Current.CancellationToken);
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"`{method.ToUpper()} {path}` should require authorization");

        // Simulate a request with a faulty token
        var req2 = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), path.Replace("{id}", Guid.NewGuid().ToString()));
        req2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "this_is_not.a_valid.token");
        var result2 = await _client.SendAsync(req2, TestContext.Current.CancellationToken);
        result2.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"`{method.ToUpper()} {path}` should return 401 with a faulty token");
    }
    // ------------------------- End Authentication Tests ---------------------
}



[CollectionDefinition(MinimalApiEndpointsValidator.CollectionName)]
public class MinimalApiEndpointsCollection : ICollectionFixture<RecipeApiFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
