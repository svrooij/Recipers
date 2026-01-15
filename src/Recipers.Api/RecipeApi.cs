using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.OpenApi;
namespace Recipers.Api;

public static class RecipeApi
{
  public static void MapRecipeApi(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/recipes")
      .WithTags("Recipes")
      //.RequireAuthorization()
      ;

    group.MapGet("/", async (IRecipeService service) =>
      Results.Ok(await service.GetRecipesAsync()))
      .WithName("GetRecipes")
      .Produces<IEnumerable<Recipe>>(StatusCodes.Status200OK)
      .AddOpenApiOperationTransformer((operation, _, _) =>
      {
        operation.Summary = "Get all recipes";
        operation.Description = "Returns all public recipes and private recipes owned by the user.";
        return Task.CompletedTask;
      })
      .RequireAuthorization()
      ;

    group.MapGet("/{id:guid}", async (string id, IRecipeService service) =>
    {
      var recipe = await service.GetRecipeAsync(id);
      return recipe is not null ? Results.Ok(recipe) : Results.NotFound();
    })
    .WithName("GetRecipe")
    .Produces<Recipe>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .AddOpenApiOperationTransformer((operation, _, _) =>
    {
      operation.Summary = "Get a recipe by id";
      operation.Description = "Returns a recipe if it is public or owned by the user.";
        return Task.CompletedTask;
    })
      .RequireAuthorization()
      ;

    group.MapPost("/", async (Recipe recipe, IRecipeService service) =>
      Results.Ok(await service.CreateRecipeAsync(recipe)))
      .WithName("CreateRecipe")
      .Produces<Recipe>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .AddOpenApiOperationTransformer((operation, _, _) =>
      {
        operation.Summary = "Create a new recipe";
        operation.Description = "Creates a new recipe. The UserId is set from the authenticated user's oid claim.";
        return Task.CompletedTask;
      })
      .RequireAuthorization()
      ;

    group.MapPut("/{id:guid}", async (string id, Recipe recipe, IRecipeService service) =>
    {
      var updated = await service.UpdateRecipeAsync(id, recipe);
      return updated is not null ? Results.Ok(updated) : Results.NotFound();
    })
    .WithName("UpdateRecipe")
    .Produces<Recipe>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .AddOpenApiOperationTransformer((operation, _, _) =>
    {
      operation.Summary = "Update a recipe";
      operation.Description = "Updates a recipe if it is public or owned by the user. The UserId is set from the authenticated user's oid claim.";
      return Task.CompletedTask;
    })
    .RequireAuthorization()
    ;

    group.MapDelete("/{id:guid}", async (string id, IRecipeService service) =>
      await service.DeleteRecipeAsync(id) ? Results.NoContent() : Results.NotFound())
      .WithName("DeleteRecipe")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .AddOpenApiOperationTransformer((operation, _, _) =>
      {
        operation.Summary = "Delete a recipe";
        operation.Description = "Deletes a recipe if it is public or owned by the user.";
        return Task.CompletedTask;
      })
      //.RequireAuthorization()
      ;
  }
}
