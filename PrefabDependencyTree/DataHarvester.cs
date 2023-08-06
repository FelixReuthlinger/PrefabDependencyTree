using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Jotunn.Managers;
using PrefabDependencyTree.Model;

namespace PrefabDependencyTree;

public static class DataHarvester
{
    public static Dictionary<string, SimpleItem> Items = null!;
    public static Dictionary<string, SimpleCraftingStation> CraftingStations = null!;
    public static Dictionary<string, SimpleRecipe> Recipes = null!;
    
    public static void Initialize()
    {
        Items = PrefabManager.Cache
            .GetPrefabs(typeof(ItemDrop))
            .ToDictionary(
                kv => kv.Key,
                kv => SimpleItem.FromItemDrop((ItemDrop)kv.Value)
            );
        Logger.LogInfo($"loaded {Items.Count} items from game");

        CraftingStations = SimpleCraftingStation.FromStationExtensions(
            PrefabManager.Cache.GetPrefabs(typeof(StationExtension))
                .Select(kv => (StationExtension)kv.Value)
                .ToList()
        );
        Logger.LogInfo($"loaded {CraftingStations.Count} crafting stations from game");

        Recipes = PrefabManager.Cache.GetPrefabs(typeof(Recipe))
            .ToDictionary(kv => kv.Key, kv => (Recipe)kv.Value)
            .Where(kv => kv.Value.m_item != null)
            .ToDictionary(
                kv => kv.Key,
                kv => SimpleRecipe.FromRecipe(kv.Value)
            );
        Logger.LogInfo($"loaded {Recipes.Count} recipes from game");
    }
}