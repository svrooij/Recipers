using System.Net;
using Recipers.Api.Tests.Helpers;
using FluentAssertions;
using System.Runtime.CompilerServices;
namespace Recipers.Api.TUnitTests;

public class MinimalApiEndpointsValidator
{

    private static RecipeApiFactory _factory;
    private static HttpClient _client;

    public MinimalApiEndpointsValidator()
    {

    }

    [Before(HookType.Class)]
    public static void Setup()
    {
        _factory = new RecipeApiFactory();
        _client = _factory.CreateClient();
    }

    [After(HookType.Class)]
    public static void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Recipes_endpoint_should_return_statuscode_401_without_token()
    {
        // Test that endpoints require authorization
        var response = await _client.GetAsync("/recipes", TestContext.Current!.Execution.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task WeatherForecast_endpoint_should_return_statuscode_401_without_token()
    {
        // Test that endpoints require authorization
        var response = await _client.GetAsync("/weatherforecast", TestContext.Current!.Execution.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task NotExisting_endpoint_should_return_statuscode_401_without_token()
    {
        // Test that endpoints require authorization
        var response = await _client.GetAsync("/notexisting", TestContext.Current!.Execution.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Retrieves all endpoints from the OpenAPI documentation.
    /// This method fetches the OpenAPI JSON document, parses it, and yields each endpoint
    /// </summary>
    /// <returns></returns>
    public static async IAsyncEnumerable<Func<EndpointDefinition>> GetAllEndpointsUsingOpenApiSpec([EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        // Because this is a static method, we need to create a new factory instance
        var factory = new RecipeApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var paths = doc.RootElement.GetProperty("paths");
        foreach (var pathProp in paths.EnumerateObject())
        {
            foreach (var methodProp in pathProp.Value.EnumerateObject())
            {
                yield return () => new EndpointDefinition(methodProp.Name, pathProp.Name);
            }
        }
    }

    // ------------------------- Authentication Tests -------------------------
    [Test]
    [MethodDataSource(nameof(GetAllEndpointsUsingOpenApiSpec))]
    public async Task Endpoint_should_return_statuscode_401_without_valid_token(EndpointDefinition endpointDefinition)
    {
        // Take the path and method from the OpenAPI spec,
        // replace any {id} placeholders with a random GUID (all ID's are GUIDs in this API) change to your situation,
        // undefined routes return 404, if you did not set up RequireAuthorization on the entire API.
        var urlPath = endpointDefinition.Path.Replace("{id}", Guid.NewGuid().ToString());
        var req = new HttpRequestMessage(new HttpMethod(endpointDefinition.Method.ToUpperInvariant()), urlPath);
        var result = await _client.SendAsync(req, TestContext.Current!.Execution.CancellationToken);
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"`{endpointDefinition.Method.ToUpper()} {endpointDefinition.Path}` should require authorization");

        // Simulate a request with a faulty token
        var req2 = new HttpRequestMessage(new HttpMethod(endpointDefinition.Method.ToUpperInvariant()), urlPath);
        req2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "this_is_not.a_valid.token");
        var result2 = await _client.SendAsync(req2, TestContext.Current!.Execution.CancellationToken);
        result2.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"`{endpointDefinition.Method.ToUpper()} {endpointDefinition.Path}` should return 401 with a faulty token");
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    // ------------------------- End Authentication Tests ---------------------

    public record EndpointDefinition(string Method, string Path);
}

