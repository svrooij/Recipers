using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.OpenApi;
namespace Recipers.Api;

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