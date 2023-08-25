using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Model;

public class BaseCrafting
{
    public readonly string Name;
    public readonly Dictionary<string, GraphRecipe> Recipes = new();

    protected BaseCrafting(string name)
    {
        Name = name;
    }

    protected BaseCrafting(string name, Dictionary<string, GraphRecipe> recipes)
    {
        Name = name;
        Recipes = recipes;
    }

    public bool ContainsItemTypes(List<string> itemTypes)
    {
        return Recipes
            .Any(recipe =>
                itemTypes.Contains(recipe.Value.CraftedItem.Item1.ItemType)
                || recipe.Value.RequiredItems.Any(requiredItem =>
                    itemTypes.Contains(requiredItem.Key.ItemType)));
    }

    public void RemoveRecipesForTypes(List<string> itemTypes)
    {
        int completeRecipeCount = Recipes.Count;
        var reducedRecipes = Recipes
            .Where(recipe =>
                !itemTypes.Contains(recipe.Value.CraftedItem.Item1.ItemType)
                && !recipe.Value.RequiredItems.Any(item =>
                    itemTypes.Contains(item.Key.ItemType))
            )
            .ToDictionary(
                recipe => recipe.Key,
                recipe => recipe.Value
            );
        Recipes.Clear();
        Recipes.AddRange(reducedRecipes);
        Logger.LogInfo(
            $"reduced '{Name}' recipes " +
            $"from original count {completeRecipeCount} to reduced count {Recipes.Count}");
    }

    public override string ToString()
    {
        string recipes = string.Join("\n    ",
            Recipes.Select(recipe => 
                recipe.Value.ToString().Replace("\n", "\n    "))
        );
        return $"name '{Name}' has recipes:\n" +
               $"    {recipes}";
    }
}