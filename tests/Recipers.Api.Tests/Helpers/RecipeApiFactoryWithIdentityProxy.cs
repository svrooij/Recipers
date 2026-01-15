using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.IdentityProxy;

namespace Recipers.Api.Tests.Helpers;

/// <summary>
/// RecipeApiFactoryWithIdentityProxy is a custom WebApplicationFactory that sets up an ASP.NET Core application
/// for testing with an Identity Proxy. It allows for integration tests that require authentication.
/// </summary>
/// <remarks>
/// Testcontainers are used to run the Identity Proxy in a Docker container.
/// The Identity Proxy acts as an authentication server, allowing tests to obtain JWT tokens.
/// </remarks>
/// <seealso cref="WebApplicationFactory{TEntryPoint}"/>
public class RecipeApiFactoryWithIdentityProxy : WebApplicationFactory<IWebApiMarker>, IAsyncLifetime
{
    // We need the authority up-front, so this is the only thing we cannot change dynamically
    private const string AUTHORITY = "https://login.microsoftonline.com/common/v2.0";

    // 👇 1️⃣ Setup identity proxy, see https://github.com/svrooij/identityproxy
    private readonly IdentityProxyContainer _identityProxyContainer = new IdentityProxyBuilder()
        .WithImage("ghcr.io/svrooij/identityproxy:v0.2.0")
        .WithAuthority(AUTHORITY)
        .WithLogger(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance) // No logging from docker container
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 👇 2️⃣ Override the JWT:Authority with the value from the proxy
        builder.UseSetting("JWT:Authority", _identityProxyContainer.GetAuthority());
        builder.UseSetting("JWT:RequireHttpsMetadata", "false");

        // Check configuration settings
        builder.ConfigureAppConfiguration((context, config) =>
        {

        });

        // Override services if needed
        builder.ConfigureTestServices(services =>
        {
            // Example: services.AddSingleton<IRecipeService, MockRecipeService>();
            // You can add mock services or replace existing ones here
        });
    }

    // 👇3️⃣ Expose the IdentityProxy GetTokenAsync method
    /// <summary>
    /// Asynchronously retrieves a token from the Identity Proxy.
    /// This method is used to obtain a token for testing purposes.
    /// </summary>
    /// <param name="tokenRequest">What claims do you want in the token?</param>
    /// <returns><see cref="TokenResult"/> with AccessToken and ExpiresIn</returns>
    internal async Task<TokenResult?> GetTokenAsync(TokenRequest tokenRequest)
    {
        return await _identityProxyContainer.GetTokenAsync(tokenRequest);
    }

    // 👇4️⃣ Make sure the container is started on initialize
    /// <summary>
    /// Asynchronously initializes the RecipeApiFactoryWithIdentityProxy, which calls the testcontainer library to start a docker container.
    /// </summary>
    /// <remarks>Be sure docker is running before calling this method.</remarks>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask InitializeAsync()
    {
        await _identityProxyContainer.StartAsync();
    }

    // 👇5️⃣ Dispose of the container after all tests have run
    /// <summary>
    /// Asynchronously disposes of the RecipeApiFactoryWithIdentityProxy, which stops the docker container.
    /// This method is called after all tests have run.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        await _identityProxyContainer.DisposeAsync();
        await base.DisposeAsync();
    }

}