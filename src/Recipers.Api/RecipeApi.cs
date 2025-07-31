using System.Collections.Concurrent;
using System.Security.Claims;

namespace Recipers.Api;

public static class RecipeApi
{
  public static void MapRecipeApi(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/recipes")
      //.RequireAuthorization()
      ;

    group.MapGet("/", async (IRecipeService service) =>
      Results.Ok(await service.GetRecipesAsync()))
      .WithName("GetRecipes")
      .Produces<IEnumerable<Recipe>>(StatusCodes.Status200OK)
      .WithOpenApi(operation =>
      {
        operation.Summary = "Get all recipes";
        operation.Description = "Returns all public recipes and private recipes owned by the user.";
        operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");

        return operation;
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
    .WithOpenApi(operation =>
    {
      operation.Summary = "Get a recipe by id";
      operation.Description = "Returns a recipe if it is public or owned by the user.";
      operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
      return operation;
    })
      .RequireAuthorization()
      ;

    group.MapPost("/", async (Recipe recipe, IRecipeService service) =>
      Results.Ok(await service.CreateRecipeAsync(recipe)))
      .WithName("CreateRecipe")
      .Produces<Recipe>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .WithOpenApi(operation =>
      {
        operation.Summary = "Create a new recipe";
        operation.Description = "Creates a new recipe. The UserId is set from the authenticated user's oid claim.";
        operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
        return operation;
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
    .WithOpenApi(operation =>
    {
      operation.Summary = "Update a recipe";
      operation.Description = "Updates a recipe if it is public or owned by the user. The UserId is set from the authenticated user's oid claim.";
      operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
      return operation;
    })
    .RequireAuthorization()
    ;

    group.MapDelete("/{id:guid}", async (string id, IRecipeService service) =>
      await service.DeleteRecipeAsync(id) ? Results.NoContent() : Results.NotFound())
      .WithName("DeleteRecipe")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .WithOpenApi(operation =>
      {
        operation.Summary = "Delete a recipe";
        operation.Description = "Deletes a recipe if it is public or owned by the user.";
        operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
        return operation;
      })
      //.RequireAuthorization()
      ;
  }


}

public record Recipe(Guid? Id, string Name, string Ingredients, string Instructions, bool? IsPrivate, string? UserId);

public interface IRecipeService
{
  Task<IEnumerable<Recipe>> GetRecipesAsync();
  Task<Recipe?> GetRecipeAsync(string id);
  Task<Recipe> CreateRecipeAsync(Recipe recipe);
  Task<Recipe?> UpdateRecipeAsync(string id, Recipe recipe);
  Task<bool> DeleteRecipeAsync(string id);
}

public class InMemoryRecipeService : IRecipeService
{
  private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, Recipe> _recipes = new();
    private readonly object _initLock = new();
    private bool _initialized = false;

  public InMemoryRecipeService(IHttpContextAccessor httpContextAccessor)
  {
    EnsureInitialized();
    _httpContextAccessor = httpContextAccessor;
  }

  private void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;
            // Pre-load 4 static recipes with given names and static GUIDs
            var staticRecipes = new[]
            {
                new Recipe(
                    Guid.Parse("fd4a5be7-a098-45d2-bfa9-6c748fc61037"),
                    "Secret Sauce",
                    "Tomatoes, Vinegar, Sugar, Spices",
                    "Mix all ingredients and simmer for 30 minutes.",
                    true,
                    "fd4a5be7-a098-45d2-bfa9-6c748fc61037"
                ),
                new Recipe(
                    Guid.Parse("b1e1e1e1-1111-2222-3333-444444444444"),
                    "Chilly Braadwurst",
                    "Braadwurst, Chili Peppers, Onions, Buns",
                    "Grill braadwurst, sauté onions and chili, serve in buns.",
                    false,
                    null
                ),
                new Recipe(
                    Guid.Parse("c2c2c2c2-2222-3333-4444-555555555555"),
                    "Beef Beer Stew",
                    "Beef, Beer, Carrots, Potatoes, Onions",
                    "Brown beef, add vegetables and beer, simmer until tender.",
                    false,
                    null
                ),
                new Recipe(
                    Guid.Parse("d3d3d3d3-3333-4444-5555-666666666666"),
                    "Sauerkraut special",
                    "Sauerkraut, Pork, Juniper Berries, Potatoes",
                    "Layer pork and sauerkraut with spices, bake with potatoes.",
                    true,
                    "fd4a5be7-a098-45d2-bfa9-6c748fc61037"
                )
            };
            for (int i = 0; i < staticRecipes.Length; i++)
            {
              var id = staticRecipes[i].Id?.ToString() ?? Guid.NewGuid().ToString();
                _recipes.TryAdd(id, staticRecipes[i]);
            }
            _initialized = true;
        }
    }

    public Task<IEnumerable<Recipe>> GetRecipesAsync()
    {
        var userId = GetCurrentUserId();
        var result = _recipes
            .Where(kvp => !kvp.Value.IsPrivate == true || (kvp.Value.UserId == userId))
            .Select(kvp => kvp.Value);
        return Task.FromResult(result);
    }

    public Task<Recipe?> GetRecipeAsync(string id)
    {
        var userId = GetCurrentUserId();
        if (_recipes.TryGetValue(id, out var recipe))
        {
            if (recipe.IsPrivate != true || recipe.UserId == userId)
                return Task.FromResult<Recipe?>(recipe);
        }
        return Task.FromResult<Recipe?>(null);
    }

    public Task<Recipe> CreateRecipeAsync(Recipe recipe)
    {
        var userId = GetCurrentUserId();
        var newId = Guid.NewGuid();
        var newRecipe = recipe with { Id = newId, UserId = userId };
        _recipes[newId.ToString()] = newRecipe;
        return Task.FromResult(newRecipe);
    }

    public Task<Recipe?> UpdateRecipeAsync(string id, Recipe recipe)
    {
        var userId = GetCurrentUserId();
        if (_recipes.TryGetValue(id, out var existing))
        {
            if (existing.IsPrivate != true || existing.UserId == userId)
            {
                var updated = recipe with { Id = existing.Id, UserId = userId };
                _recipes[id] = updated;
                return Task.FromResult<Recipe?>(updated);
            }
        }
        return Task.FromResult<Recipe?>(null);
    }

    public Task<bool> DeleteRecipeAsync(string id)
    {
        var userId = GetCurrentUserId();
        if (_recipes.TryGetValue(id, out var existing))
        {
            if (existing.IsPrivate != true || existing.UserId == userId)
            {
                return Task.FromResult(_recipes.TryRemove(id, out _));
            }
        }
        return Task.FromResult(false);
    }

    private string? GetCurrentUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        return principal?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
    }
}