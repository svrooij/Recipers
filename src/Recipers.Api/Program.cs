using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(api =>
{
    api.AddDocumentTransformer<SecurityDocumentTransformer>();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Bind the JWT section in the configuration to JwtBearerOptions
    builder.Configuration.Bind("JWT", options);

    // Override some options we don't want to set in appsettings.json
    options.TokenValidationParameters.ValidateIssuer = false; // Disable issuer validation for common endpoint
    options.TokenValidationParameters.ValidateAudience = true; // Enable audience validation
});

builder.Services.AddAuthorization(options =>
{
    // Add policies or configure authorization options if needed
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddOAuth2Authentication("jwt", auth =>
        {
            // Bind the ScalarOAuth section in the configuration to OAuth2AuthenticationOptions
            builder.Configuration.Bind("ScalarOAuth", auth);
            auth.Description = "Entra ID authentication using JWT Bearer tokens";
            auth.WithFlows(flows =>
            {
                flows.AuthorizationCode = new AuthorizationCodeFlow();
                builder.Configuration.Bind("ScalarOAuthCodeFlow", flows.AuthorizationCode);
                flows.AuthorizationCode.Pkce = Pkce.Sha256; // Set PKCE
            });

        });
        options.AddPreferredSecuritySchemes("jwt");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization()
.WithOpenApi(operation =>
{
    operation.Summary = "Get weather forecast";
    operation.Description = "Returns a 5-day weather forecast with random temperatures and summaries.";
    // Add security requirements for the operation (extension method to make it easier)
    operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
    // Alternatively, you can add security requirements directly
    // operation.Security.Add(new OpenApiSecurityRequirement
    // {
    //     {new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "jwt" }}, new[] { "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation" } }
    // });
    return operation;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
