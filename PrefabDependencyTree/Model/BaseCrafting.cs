using System.Collections.Generic;

namespace PrefabDependencyTree.Model;

public class BaseCrafting
{
    protected readonly string Name;
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
}