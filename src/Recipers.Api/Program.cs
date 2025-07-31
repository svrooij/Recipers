
using Scalar.AspNetCore;
using Recipers.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(api =>
{
    api.AddDocumentTransformer<SecurityDocumentTransformer>();
});

// ------------------- Authentication and Authorization -------------------
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
// ------------------- End Authentication and Authorization -------------------

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IRecipeService, InMemoryRecipeService>();

var app = builder.Build();

// Enable OpenAPI documentation for development and testing environments
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

// Map the Weather API endpoints
app.MapGet("/", () => "Welcome to the Recipers API! Documentation is available at /scalar/").ExcludeFromDescription();
app.MapWeatherApi();
app.MapRecipeApi();

app.Run();
