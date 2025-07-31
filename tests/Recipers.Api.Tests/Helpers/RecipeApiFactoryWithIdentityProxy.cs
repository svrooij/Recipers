using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.IdentityProxy;

namespace Recipers.Api.Tests.Helpers;

public class RecipeApiFactoryWithIdentityProxy : WebApplicationFactory<IWebApiMarker>, IAsyncLifetime
{
    // We need the authority up-front, so this is the only thing we cannot change dynamically
    private const string AUTHORITY = "https://login.microsoftonline.com/common/v2.0";
    private readonly IdentityProxyContainer _identityProxyContainer = new IdentityProxyBuilder()
        .WithImage("ghcr.io/svrooij/identityproxy:v0.1.4")
        .WithAuthority(AUTHORITY)
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Change the Authority to the Identity Proxy
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

    internal async Task<TokenResult?> GetTokenAsync(TokenRequest tokenRequest)
    {
        return await _identityProxyContainer.GetTokenAsync(tokenRequest);
    }

    public async ValueTask InitializeAsync()
    {
        await _identityProxyContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _identityProxyContainer.DisposeAsync();
        await base.DisposeAsync();
    }

}