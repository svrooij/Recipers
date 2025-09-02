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
}

[CollectionDefinition(RecipeTests.CollectionName)]
public class ControllerCollection : ICollectionFixture<RecipeApiFactoryWithIdentityProxy>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}