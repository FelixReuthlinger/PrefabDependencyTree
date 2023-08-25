using System.Collections.Generic;
using System.Security.Permissions;

namespace PrefabDependencyTree.Model;

public class GraphProcessor : BaseCrafting
{
    private GraphProcessor(string name, Dictionary<string, GraphRecipe> recipes) : base(name, recipes)
    {
    }

    public static GraphProcessor FromSmelter(Smelter smelter)
    {
        return new GraphProcessor(smelter.name, GraphRecipe.FromSmelter(smelter));
    }

    public static GraphProcessor FromIncinerator(Incinerator incinerator)
    {
        return new GraphProcessor(incinerator.name, GraphRecipe.FromIncinerator(incinerator));
    }

    public static GraphProcessor FromFermenter(Fermenter fermenter)
    {
        return new GraphProcessor(fermenter.name, GraphRecipe.FromFermenter(fermenter));
    }

    public static GraphProcessor FromCookingStation(CookingStation cookingStation)
    {
        return new GraphProcessor(cookingStation.name, GraphRecipe.FromCookingStation(cookingStation));
    }

    public override string ToString()
    {
        return $"[processor {base.ToString()}\n" +
               $"]";
    }
}