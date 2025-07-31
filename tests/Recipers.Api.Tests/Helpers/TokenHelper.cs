using Testcontainers.IdentityProxy;

internal static class TokenHelper
{
  public static TokenRequest CreateTokenRequestForStephan()
  {
    return CreateTokenRequest("vnp3d6ojtfrNjY0uAbZzyQFClFGsMEQ3Cgpb9DlVaVo", "fd4a5be7-a098-45d2-bfa9-6c748fc61037");
  }

  public static TokenRequest CreateTokenRequest(string subject, string objectId)
  {
    return new TokenRequest
    {
      Audience = "c8ccec6e-9f74-4de1-a6cd-18e665c3e685", // Replace with your actual audience
      Subject = subject, // This is the user identifier, e.g., "
      Issuer = "https://login.microsoftonline.com/df68aa03-48eb-4b09-9f3e-8aecc58e207c/v2.0", // Replace with your actual issuer
      AdditionalClaims = new Dictionary<string, object>
      {
        { "oid", objectId }, // Object ID of the user in Entra ID
        { "name", "Test User" }, // Optional: Name of the user
        { "preferred_username", "fake@svrooij.io" }, // Optional: Username of the user
        { "scp", "user-impersonation" } // Optional: Scope of the token
      }
    };
  }


}

/**
This is what an actual token from entra for this application looks like:
The audience (aud) and the issuer (iss) are important and the (oid) is the object ID of the user in Entra ID,
this is used to tie to personal recipes

{
  "aud": "c8ccec6e-9f74-4de1-a6cd-18e665c3e685",
  "iss": "https://login.microsoftonline.com/df68aa03-48eb-4b09-9f3e-8aecc58e207c/v2.0",
  "iat": 1753968177,
  "nbf": 1753968177,
  "exp": 1753972591,
  "aio": "ATQBy/4ZAAAAgTbjrJuT9hYRXpZwNTxjRltHsgond1iQt8jyDj8BfTahHUpzUbX9vgTrGFXDmbjmpWjQNBC8royfNyIx5NkHrqhF8hWipWkoDTJHLU7UJ0BaEs70MKjlW67X4xVB8pOF8giSN0EGU32CY6HZ7GYs5fYHkzs5zFAIcU9d4FT/+vytd0mlJv3TpI3kHot9XMU4h11hTCZa1/yo3jlmIb7O9lE6yaBuNAresZjBlyDcX746mxAJe7rS329zVHYkfwe2I43W7FYzeHFFGdiccqYZdtjHPmpEBfhe6Ww8X3yPChZmLyYvUzRG6y6TthNjS06DJ8IWQem5v15oJ3FU3xrJ2OT6skkfn0o1aIoPjf0cSTQmOQ2wiUmiDxv9kHAqjbNjNrHwhw6tsjB00be2kMh9ZA==",
  "azp": "6a1af9c0-ea5e-484b-8682-a7d05ea16506",
  "azpacr": "0",
  "name": "Stephan van Rooij",
  "oid": "fd4a5be7-a098-45d2-bfa9-6c748fc61037",
  "preferred_username": "fake@svrooij.io",
  "rh": "1.AUsAA6po3-tICUufPorsxY4gfG7szMh0n-FNps0Y5mXD5oUZAT9LAA.",
  "scp": "user-impersonation",
  "sid": "c9aefeed-35ca-453d-b02c-ac6a6c9e65cf",
  "sub": "vnp3d6ojtfrNjY0uAbZzyQFClFGsMEQ3Cgpb9DlVaVo",
  "tid": "df68aa03-48eb-4b09-9f3e-8aecc58e207c",
  "uti": "xxx",
  "ver": "2.0",
  "xms_ftd": "xxx"
}

**/