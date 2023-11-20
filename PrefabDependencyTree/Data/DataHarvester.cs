using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Model;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data;

public static class DataHarvester
{
    public static readonly Dictionary<string, GraphItem> Items = new();
    public static readonly Dictionary<string, GraphRecipe> UnboundRecipes = new();
    public static readonly Dictionary<string, GraphCraftingStation> CraftingStations = new();
    public static readonly Dictionary<string, GraphProcessor> Processors = new();
    public static readonly Dictionary<Tuple<string, DropType>, List<GraphItem>> Drops = new();
    public static readonly Dictionary<string, GraphPiece> Pieces = new();

    public static void Initialize()
    {
        InitializeSmelters();
        InitializeIncinerator();
        InitializeCookingStations();
        InitializeFermenters();
        InitializeCraftingStations();
        InitializePieces();
        InitializeRecipes();
        InitializeDrops();
        // show results
        LogOverview();
        LogItemTypesOverview();
    }

    private static void LogOverview()
    {
        Logger.LogInfo("vvvv data harvester overview vvvv");
        Logger.LogInfo($"    total {Items.Count} items registered");
        Logger.LogInfo($"    total {Drops.Count} drops registered");
        Logger.LogInfo($"    total {Pieces.Count} pieces registered");
        Logger.LogInfo($"    total {CraftingStations.Count} crafting stations registered");
        Logger.LogInfo($"    total {Processors.Count} processor stations (fermenter, smelter, ...) registered");
        Logger.LogInfo($"    total {UnboundRecipes.Count} unbound recipes registered");
        Logger.LogInfo("^^^^ data harvester overview ^^^^");
    }

    private static void LogItemTypesOverview()
    {
        Dictionary<string, int> itemTypeCounts = Items
            .GroupBy(item => item.Value.ItemType)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            );

        Logger.LogInfo("vvvv item types overview vvvv");
        foreach (KeyValuePair<string, int> pair in itemTypeCounts.OrderBy(pair => pair.Key))
        {
            Logger.LogInfo($"item type '{pair.Key}' -> found {pair.Value} items");
        }

        Logger.LogInfo("^^^^ item types overview ^^^^");
    }

    public static List<string> LogAllToString()
    {
        List<string> output = new List<string> { "==== debug log output for all prefabs ====" };
        output.AddRange(Items.Select(item => item.Value.ToString()));
        output.AddRange(
            from drop in Drops
            let dropsList = string.Join("\n    ", drop.Value.Select(item => item.ToString()))
            select $"['{drop.Key}' drops:\n" + $"    {dropsList}\n" + $"]"
        );
        output.AddRange(Pieces.Select(piece => piece.Value.ToString()));
        output.AddRange(CraftingStations.Select(station => station.Value.ToString()));
        output.AddRange(Processors.Select(processor => processor.Value.ToString()));
        output.AddRange(UnboundRecipes.Select(recipe => recipe.Value.ToString()));
        output.Add("==== debug log output for all prefabs ====");
        return output;
    }

    public static List<string> LogAllItemsToCategorizedYaml()
    {
        List<GraphRecipe> recipes = CraftingStations
            .SelectMany(cs => cs.Value.Recipes
                .Select(recipe => recipe.Value)
                .ToList()
            ).Union(UnboundRecipes.Select(unbound => unbound.Value))
            .Union(Processors.SelectMany(processor =>
                processor.Value.Recipes.Select(recipe => recipe.Value).ToList()))
            .ToList();
        List<GraphItem> itemsFromRecipes = recipes.SelectMany(recipe =>
        {
            var itemsResults = new List<GraphItem> { recipe.CraftedItem.Item1 };
            itemsResults.AddRange(recipe.RequiredItems.Select(req => req.Key));
            return itemsResults;
        }).ToList();
        var allItems = Items.Select(item => item.Value).Union(itemsFromRecipes)
            .Union(Drops.SelectMany(drop => drop.Value)).ToList();
        Dictionary<string, List<string>> items = allItems.GroupBy(item => item.ItemType)
            .ToDictionary(group => group.Key, 
                group => group.Select(item => item.ItemName).Distinct().OrderBy(item => item).ToList());
        List<string> output = new List<string>();
        foreach (var group in items)
        {
            output.Add($"{group.Key}:");
            output.AddRange(group.Value.Select(item => $"  - {item}"));
        }

        return output;
    }

    private static void InitializePieces()
    {
        Dictionary<string, GraphPiece> pieces = PrefabManager.Cache.GetPrefabs(typeof(Piece)).ToDictionary(
            kv => kv.Key,
            kv => GraphPiece.FromPiece((Piece)kv.Value)
        );
        pieces
            .Where(kv =>
                !CraftingStations.ContainsKey(kv.Key) && !Processors.ContainsKey(kv.Key))
            .ToList()
            .ForEach(piece => Pieces.Add(piece.Key, piece.Value));
        Logger.LogInfo($"loaded {pieces.Count} pieces from game");
    }

    private static void InitializeSmelters()
    {
        Dictionary<string, GraphProcessor> smelters = PrefabManager.Cache.GetPrefabs(typeof(Smelter)).ToDictionary(
            kv => kv.Key,
            kv => GraphProcessor.FromSmelter((Smelter)kv.Value)
        );
        smelters.ToList().ForEach(smelter => Processors.Add(smelter.Key, smelter.Value));
        int recipeCountTotal = smelters
            .Select(smelter => smelter.Value.Recipes.Count)
            .Sum();
        Logger.LogInfo($"loaded {smelters.Count} smelters with {recipeCountTotal} conversions from game");
    }

    private static void InitializeIncinerator()
    {
        Dictionary<string, GraphProcessor> incinerators = PrefabManager.Cache.GetPrefabs(typeof(Incinerator))
            .ToDictionary(
                kv => kv.Key,
                kv => GraphProcessor.FromIncinerator((Incinerator)kv.Value)
            );
        incinerators.ToList().ForEach(incinerator =>
            Processors.Add(incinerator.Key, incinerator.Value));
        int recipeCountTotal = incinerators
            .Select(incinerator => incinerator.Value.Recipes.Count)
            .Sum();
        Logger.LogInfo($"loaded {incinerators.Count} incinerators with {recipeCountTotal} conversions from game");
    }

    private static void InitializeCookingStations()
    {
        var cookingStations = PrefabManager.Cache
            .GetPrefabs(typeof(CookingStation)).ToDictionary(
                kv => kv.Key,
                kv => GraphProcessor.FromCookingStation((CookingStation)kv.Value)
            );
        cookingStations.ToList().ForEach(fermenter
            => Processors.Add(fermenter.Key, fermenter.Value));
        int recipeCountTotal = cookingStations
            .Select(smelter => smelter.Value.Recipes.Count)
            .Sum();
        Logger.LogInfo(
            $"loaded {cookingStations.Count} cooking stations with {recipeCountTotal} conversions from game");
    }

    private static void InitializeFermenters()
    {
        var fermenters = PrefabManager.Cache.GetPrefabs(typeof(Fermenter)).ToDictionary(
            kv => kv.Key,
            kv => GraphProcessor.FromFermenter((Fermenter)kv.Value)
        );
        fermenters.ToList().ForEach(fermenter
            => Processors.Add(fermenter.Key, fermenter.Value));
        int recipeCountTotal = fermenters
            .Select(smelter => smelter.Value.Recipes.Count)
            .Sum();
        Logger.LogInfo(
            $"loaded {fermenters.Count} fermenters with {recipeCountTotal} conversions from game");
    }

    private static void InitializeCraftingStations()
    {
        Dictionary<string, GraphCraftingStation> newStations = GraphCraftingStation.FromExtensionsAndStations(
            extensions: PrefabManager.Cache.GetPrefabs(typeof(StationExtension))
                .Select(kv => (StationExtension)kv.Value)
                .ToList(),
            stations: PrefabManager.Cache.GetPrefabs(typeof(CraftingStation))
                .Select(kv => (CraftingStation)kv.Value)
                .ToList()
        );
        newStations.ToList().ForEach(station
            => CraftingStations.Add(station.Key, station.Value));
        Logger.LogInfo($"loaded {newStations.Count} crafting stations from game");
    }

    private static void InitializeRecipes()
    {
        Dictionary<string, Recipe> recipes = PrefabManager.Cache.GetPrefabs(typeof(Recipe))
            .ToDictionary(kv => kv.Key, kv => (Recipe)kv.Value);

        foreach (KeyValuePair<string, Recipe> pair in recipes)
        {
            if (pair.Value.m_item == null)
            {
                Logger.LogInfo($"recipe '{pair.Key}' does not create an item - skipping");
                continue;
            }

            if (pair.Value.m_craftingStation != null)
            {
                if (CraftingStations.TryGetValue(pair.Value.m_craftingStation.name, out GraphCraftingStation station))
                    station.Recipes.Add(pair.Key, GraphRecipe.FromRecipe(pair.Value));
                else
                    Logger.LogWarning($"recipe '{pair.Key}': " +
                                      $"station '{pair.Value.m_craftingStation.name}' not found");
            }
            else UnboundRecipes.Add(pair.Key, GraphRecipe.FromRecipe(pair.Value));
        }

        int craftingStationRecipes = CraftingStations
            .Select(station => station.Value.Recipes.Count).Sum();
        Logger.LogInfo($"loaded {craftingStationRecipes} recipes bound to crafting stations from game");
        Logger.LogInfo($"loaded {UnboundRecipes.Count} recipes from game that are not bound to a crafting station");
    }

    private static void InitializeDrops()
    {
        new List<Initializer>
        {
            new ContainerDropsInitializer(),
            new DestructibleDropsInitializer(),
            new TreeLogDropsInitializer(),
            new TreeBaseDropsInitializer(),
            new CharacterDropInitializer(),
            new MineRockDropsInitializer(),
            new MineRock5DropInitializer(),
            new LootSpawnerDropsInitializer(),
            new PickableDropInitializer(),
            new PickableExtraDropInitializer(),
            new PickableItemDropInitializer(),
            new PickableItemRandomDropInitializer()
        }.ForEach(initializer =>
        {
            try
            {
                initializer.InitializeDrops();
            }
            catch (Exception e)
            {
                Logger.LogError($"got exception on initializing: {e.Message}");
                Logger.LogError(e.StackTrace);
            }
        });
    }
}