using Recipers.Api.Tests.Helpers;
using FluentAssertions;
using Testcontainers.IdentityProxy;
using System.Net;

namespace Recipers.Api.Tests.ControllerTests;

[Collection(RecipeTests.CollectionName)]
public class RecipeTests
{
  internal const string CollectionName = "ControllerTests";
  private readonly RecipeApiFactoryWithIdentityProxy _factory;
  private readonly HttpClient _client;

  public RecipeTests(RecipeApiFactoryWithIdentityProxy factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Recipes_endpoint_should_return_statuscode_200_with_valid_token()
  {
    // Arrange: Get a valid token
    var token = await _factory.GetTokenAsync(TokenHelper.CreateTokenRequestForStephan());
    token.Should().NotBeNull();
    // Arrange: Create a request with the token
    var request = new HttpRequestMessage(HttpMethod.Get, "/recipes");
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

    // Act: Call the endpoint
    var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

    // Assert: Check the response
    response.StatusCode.Should().Be(HttpStatusCode.OK, "because the endpoint should return recipes when a valid token is provided");
    // validate the response content if needed
    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    content.Should().NotBeNullOrEmpty("because the endpoint should return a list of recipes");

    // Just checking for some strings, but you should deserialize the content to a proper object and validate it
    content.Should().Contain("Secret Sauce", "because the recipe list should contain the Secret Sauce recipe");
  }

  [Fact]
  public async Task GetRecipe_endpoint_should_return_correct_recipe()
  {
    // Arrange: Get a valid token
    var token = await _factory.GetTokenAsync(TokenHelper.CreateTokenRequestForStephan());
    token.Should().NotBeNull();
    // Arrange: Create a request with the token
    var request = new HttpRequestMessage(HttpMethod.Get, "/recipes/d3d3d3d3-3333-4444-5555-666666666666");
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

    // Act: Call the endpoint
    var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

    // Assert: Check the response
    response.StatusCode.Should().Be(HttpStatusCode.OK, "because the endpoint should return recipes when a valid token is provided");
    // validate the response content if needed
    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    content.Should().NotBeNullOrEmpty("because the endpoint should return a recipe");

    // Just checking for some strings, but you should deserialize the content to a proper object and validate it
    content.Should().Contain("Sauerkraut special", "because the Sauerkraut special recipe should be returned");
  }

  [Fact]
  public async Task GetRecipe_endpoint_should_return_404_for_private_recipe()
  {
    // Arrange: Get a valid token, for unknown user
    var token = await _factory.GetTokenAsync(TokenHelper.CreateTokenRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
    token.Should().NotBeNull();
    // Arrange: Create a request with the token
    var request = new HttpRequestMessage(HttpMethod.Get, "/recipes/d3d3d3d3-3333-4444-5555-666666666666");
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

    // Act: Call the endpoint
    var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

    // Assert: Check the response
    response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the endpoint should return 404 for private recipes when an unauthorized token is provided");
  }

  [Fact]
  public async Task GetRecipe_endpoint_with_faulty_id_should_return_404()
  {
    // Arrange: Get a valid token, for unknown user
    var token = await _factory.GetTokenAsync(TokenHelper.CreateTokenRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
    token.Should().NotBeNull();
    // Arrange: Create a request with the token
    var request = new HttpRequestMessage(HttpMethod.Get, "/recipes/this-is-not-a-valid-guid"); // This endpoint is not registered
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

    // Act: Call the endpoint
    var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

    // Assert: Check the response
    response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the endpoint should return 404 when authenticated.");
  }
}

[CollectionDefinition(RecipeTests.CollectionName)]
public class ControllerCollection : ICollectionFixture<RecipeApiFactoryWithIdentityProxy>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}