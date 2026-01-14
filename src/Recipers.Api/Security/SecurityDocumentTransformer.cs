using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public class SecurityDocumentTransformer : IOpenApiDocumentTransformer
{
  public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    // Example transformation logic
    if (document.Info != null)
    {
        document.Info.Title = "Recipers API";
        document.Info.Description = "Recipe api with JWT authentication";
    }
    document.Components ??= new OpenApiComponents();
    var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
    {
        ["jwt"] = new OpenApiSecurityScheme
        {
            // I would like to use the OpenId connect security scheme here, but it is not supported in the current version of Scalar.AspNetCore
            // Entra supports both so this is a good compromise
            Type = SecuritySchemeType.OAuth2,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Access token from Microsoft Entra",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/authorize"), // Need to load from configuration
                    TokenUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token"), // Need to load from configuration
                    Scopes = new Dictionary<string, string>
                    {
                        { "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation", "Access recipers" },
                        { "openid", "Access the OpenID Connect user profile" },
                        { "email", "Access the user's email address" },
                        { "profile", "Access the user's profile" }
                    }
                }
            }
        }
    };
    document.Components.SecuritySchemes = securitySchemes;
    document.Security = [
      new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecuritySchemeReference("jwt"), ["api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation", "openid", "profile"]
        }
      }
    ];
    // Apply security for each endpoint seperatly
    //foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
    //{
    //    operation.Value.Security ??= [];
    //    operation.Value.Security.Add(new OpenApiSecurityRequirement
    //    {
    //        [new OpenApiSecuritySchemeReference("jwt", document)] = ["api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation"]
    //    });
    //}
    document.SetReferenceHostDocument();
    return Task.CompletedTask;
  }
}