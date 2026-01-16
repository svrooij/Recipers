using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Recipers.Api.Tests.Helpers;

public class RecipeApiFactory : WebApplicationFactory<IWebApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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

}