using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
namespace Recipers.Api;

public static class LoginApi
{
  public static void MapLoginApi(this IEndpointRouteBuilder app)
  {

    app.MapPost("/api/custom-login", async ([FromBody] CustomLoginRequest login) =>
    {
      if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
      {
        return Results.ValidationProblem(new Dictionary<string, string[]>      {
          ["Username"] = new[] { "Username is required." },
          ["Password"] = new[] { "Password is required." }
        });
      }

      // For demonstration purposes, we use hardcoded credentials. In a real application, you would validate against a user store.
      if (login.Username == "testuser")
      {
        await Task.Delay(210); // Simulate async password check
        if (login.Password == "password")
        {

            // Generate a JWT token (this is a placeholder, implement your own token generation logic)
            var token = "your-fake-jwt-token"; // Replace with actual token generation logic

            return Results.Ok(token);
        }
      }

      return Results.Problem("Invalid username or password.", statusCode: StatusCodes.Status403Forbidden);
    })
      .WithName("CustomLoginEndpoint")
      .WithTags("Recipes")
      .AddOpenApiOperationTransformer((operation, _, _) =>
      {
        operation.Summary = "App login";
        operation.Description = "Logs in a user and returns a JWT token if the credentials are valid.";
        return Task.CompletedTask;
      })
      
      .Produces<string>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      
      ;
  }


}

public record CustomLoginRequest(string Username, string Password);
