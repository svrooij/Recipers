using Microsoft.OpenApi.Models;
/// <summary>
/// Extensions for OpenAPI security requirements.
/// This allows adding scopes to security requirements in a more concise way.
/// </summary>
internal static class OpenApiSecurityRequirementsExtensions
{
    /// <summary>
    /// Adds scopes to the OpenApiSecurityRequirement list.
    /// This is a convenience method to avoid repetitive code when adding scopes to security requirements.
    /// </summary>
    /// <example>
    /// ```csharp
    /// .WithOpenApi(operation =>
    /// {
    ///     operation.Summary = "Some operation";
    ///     operation.Description = "Some description.";
    ///     // Add security requirements for the operation (extension method to make it easier)
    ///     operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
    ///     return operation;
    /// });
    /// ```
    /// </example>
    /// <param name="requirements"></param>
    /// <param name="schemeName"></param>
    /// <param name="scopes"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void AddScopes(this IList<OpenApiSecurityRequirement> requirements, string schemeName, params string[] scopes)
    {
        if (requirements == null) throw new ArgumentNullException(nameof(requirements));
        if (string.IsNullOrWhiteSpace(schemeName)) throw new ArgumentException("Scheme name cannot be null or whitespace.", nameof(schemeName));
        if (scopes == null || !scopes.Any()) return;

        var securityRequirement = new OpenApiSecurityRequirement
        {
            { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = schemeName } }, scopes.ToArray() }
        };

        requirements.Add(securityRequirement);
    }
}