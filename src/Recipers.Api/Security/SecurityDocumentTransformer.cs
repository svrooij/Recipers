using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

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
    document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
    // Add security schemes to the document
    document.Components.SecuritySchemes.Add("jwt", new OpenApiSecurityScheme
    {
      // I would like to use the OpenId connect security scheme here, but it is not supported in the current version of Scalar.AspNetCore
      // Entra supports both so this is a good compromise
      Type = SecuritySchemeType.OAuth2,
      Scheme = "bearer",
      BearerFormat = "JWT",
      Description = "Use a valid JWT token to access this API.",
      In = ParameterLocation.Header,
      Name = "Authorization",
    });

    return Task.CompletedTask;
  }
}