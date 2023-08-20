using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Jotunn.Managers;
using PrefabDependencyTree.Model;

namespace PrefabDependencyTree.Data;

public static class DataHarvester
{
    public static readonly Dictionary<string, GraphItem> Items = new();
    public static readonly Dictionary<string, GraphRecipe> UnboundRecipes = new();
    public static readonly Dictionary<string, GraphCraftingStation> CraftingStations = new();
    public static readonly Dictionary<string, GraphProcessor> Processors = new();
    public static readonly Dictionary<string, List<GraphItem>> Drops = new();
    public static readonly Dictionary<string, GraphPiece> Pieces = new();

    public static void Initialize()
    {
        InitializeDrops();
        InitializePieces();
        InitializeSmelters();
        InitializeIncinerator();
        InitializeCookingStations();
        InitializeFermenters();
        InitializeCraftingStations();
        InitializeRecipes();
    }

    private static void InitializePieces()
    {
        Dictionary<string, GraphPiece> pieces = PrefabManager.Cache.GetPrefabs(typeof(Piece)).ToDictionary(
            kv => kv.Key,
            kv => GraphPiece.FromPiece((Piece)kv.Value)
        );
        pieces.ToList().ForEach(piece => Pieces.Add(piece.Key, piece.Value));
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
        Logger.LogInfo($"loaded {recipes.Count} recipes from game");

        var stationRecipes = recipes
            .Where(recipe =>
                recipe.Value.m_item != null && recipe.Value.m_craftingStation != null)
            .ToList();
        var unboundRecipes = recipes
            .Where(recipe =>
                recipe.Value.m_item != null && recipe.Value.m_craftingStation == null)
            .ToList();

        Logger.LogInfo(
            $"{stationRecipes.Count} recipes are bound to crafting stations, {unboundRecipes.Count} are not");
        foreach (KeyValuePair<string, Recipe> recipe in stationRecipes)
        {
            if (CraftingStations.TryGetValue(recipe.Value.m_craftingStation.name, out GraphCraftingStation station))
                station.Recipes.Add(recipe.Key, GraphRecipe.FromRecipe(recipe.Value));
            else Logger.LogWarning($"station '{recipe.Value.m_craftingStation.name}' not found");
        }

        foreach (KeyValuePair<string, Recipe> unboundRecipe in unboundRecipes)
        {
            UnboundRecipes.Add(unboundRecipe.Key, GraphRecipe.FromRecipe(unboundRecipe.Value));
        }
    }

    private static List<Tuple<string, string>> InitializeDropFromContainer()
    {
        List<Tuple<string, string>> containerDrops = PrefabManager.Cache.GetPrefabs(typeof(Container))
            .ToDictionary(kv => kv.Key, kv => (Container)kv.Value)
            .Where(container =>
                container.Value.m_defaultItems is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_defaultItems.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {containerDrops.Count} drops from containers from game");
        return containerDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromDestructibles()
    {
        List<Tuple<string, string>> dropsOnDestroyed = PrefabManager.Cache.GetPrefabs(typeof(DropOnDestroyed))
            .ToDictionary(kv => kv.Key, kv => (DropOnDestroyed)kv.Value)
            .Where(container =>
                container.Value.m_dropWhenDestroyed is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_dropWhenDestroyed.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {dropsOnDestroyed.Count} drops from destroyable objects from game");
        return dropsOnDestroyed;
    }

    private static List<Tuple<string, string>> InitializeDropFromLootSpawners()
    {
        List<Tuple<string, string>> lootSpawnerDrops = PrefabManager.Cache.GetPrefabs(typeof(LootSpawner))
            .ToDictionary(kv => kv.Key, kv => (LootSpawner)kv.Value)
            .Where(container =>
                container.Value.m_items is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_items.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {lootSpawnerDrops.Count} drops from loot spawners from game");
        return lootSpawnerDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromMineRock()
    {
        List<Tuple<string, string>> mineRockDrops = PrefabManager.Cache.GetPrefabs(typeof(MineRock))
            .ToDictionary(kv => kv.Key, kv => (MineRock)kv.Value)
            .Where(container =>
                container.Value.m_dropItems is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_dropItems.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {mineRockDrops.Count} drops from mine rocks from game");
        return mineRockDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromMineRock5()
    {
        List<Tuple<string, string>> mineRock5Drops = PrefabManager.Cache.GetPrefabs(typeof(MineRock5))
            .ToDictionary(kv => kv.Key, kv => (MineRock5)kv.Value)
            .Where(container =>
                container.Value.m_dropItems is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_dropItems.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {mineRock5Drops.Count} drops from other mine rocks from game");
        return mineRock5Drops;
    }

    private static List<Tuple<string, string>> InitializeDropFromPickables()
    {
        var pickables = PrefabManager.Cache.GetPrefabs(typeof(Pickable))
            .ToDictionary(kv => kv.Key, kv => (Pickable)kv.Value);
        List<Tuple<string, string>> pickableDrops = pickables
            .Where(gameObject => gameObject.Value.m_itemPrefab != null)
            .Select(gameObject =>
                Tuple.Create(gameObject.Key, gameObject.Value.m_itemPrefab.name))
            .ToList();
        List<Tuple<string, string>> pickableExtraDrops = pickables
            .Where(container =>
                container.Value.m_extraDrops is { m_drops: not null }
            )
            .SelectMany(gameObject => gameObject.Value.m_extraDrops.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name)))
            .ToList();
        var pickableAllDrops = pickableDrops.Union(pickableExtraDrops).ToList();
        Logger.LogInfo($"loaded {pickableAllDrops.Count} drops from pickables " +
            $"(including {pickableExtraDrops.Count} extra drops) from game");
        return pickableAllDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromPickableItems()
    {
        var pickableItems = PrefabManager.Cache.GetPrefabs(typeof(PickableItem))
            .ToDictionary(kv => kv.Key, kv => (PickableItem)kv.Value);
        List<Tuple<string, string>> pickableItemDrops = pickableItems
            .Where(gameObject => gameObject.Value.m_itemPrefab != null)
            .Select(gameObject =>
                Tuple.Create(gameObject.Key, gameObject.Value.m_itemPrefab.name))
            .ToList();
        List<Tuple<string, string>> pickableItemRandomDrops = pickableItems
            .Where(gameObject => gameObject.Value.m_randomItemPrefabs != null)
            .SelectMany(gameObject =>
                gameObject.Value.m_randomItemPrefabs
                    .ToList()
                    .Select(randomItem =>
                        Tuple.Create(gameObject.Key, randomItem.m_itemPrefab.name))
            ).ToList();
        var allPickableItemDrops = pickableItemDrops.Union(pickableItemRandomDrops).ToList();
        Logger.LogInfo($"loaded {allPickableItemDrops.Count} drops from pickable items (including " +
                       $"{pickableItemRandomDrops.Count} random drops) from game");
        return allPickableItemDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromTreeBase()
    {
        List<Tuple<string, string>> treeBaseDrops = PrefabManager.Cache.GetPrefabs(typeof(TreeBase))
            .ToDictionary(kv => kv.Key, kv => (TreeBase)kv.Value)
            .Where(container =>
                container.Value.m_dropWhenDestroyed is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_dropWhenDestroyed.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {treeBaseDrops.Count} drops from tree bases from game");
        return treeBaseDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromTreeLog()
    {
        List<Tuple<string, string>> treeLogDrops = PrefabManager.Cache.GetPrefabs(typeof(TreeLog))
            .ToDictionary(kv => kv.Key, kv => (TreeLog)kv.Value)
            .Where(container =>
                container.Value.m_dropWhenDestroyed is { m_drops: not null }
            ).SelectMany(gameObject => gameObject.Value.m_dropWhenDestroyed.m_drops
                .Select(drop => Tuple.Create(gameObject.Key, drop.m_item.name))
            ).ToList();
        Logger.LogInfo($"loaded {treeLogDrops.Count} drops from tree logs from game");
        return treeLogDrops;
    }

    private static List<Tuple<string, string>> InitializeDropFromCharacters()
    {
        List<Tuple<string, string>> characterDrops = PrefabManager.Cache.GetPrefabs(typeof(Character))
            .Select(kv => (Character)kv.Value)
            .ToDictionary(
                character => character.name,
                character => (CharacterDrop)character.GetComponent(typeof(CharacterDrop))
            )
            .Where(tuple => tuple.Value != null && tuple.Value.m_drops != null)
            .SelectMany(character => character.Value
                .m_drops.Select(drop => Tuple.Create(character.Key, drop.m_prefab.name))).ToList();
        Logger.LogInfo($"loaded {characterDrops.Count} drops from characters from game");
        return characterDrops;
    }

    private static void InitializeDrops()
    {
        var mergedDrops = new List<List<Tuple<string, string>>>
            {
                InitializeDropFromTreeBase(),
                InitializeDropFromTreeLog(),
                InitializeDropFromPickables(),
                InitializeDropFromPickableItems(),
                InitializeDropFromDestructibles(),
                InitializeDropFromContainer(),
                InitializeDropFromLootSpawners(),
                InitializeDropFromMineRock(),
                InitializeDropFromMineRock5(),
                InitializeDropFromCharacters()
            }.SelectMany(lists => lists)
            .GroupBy(list => list.Item1)
            .ToDictionary(
                group => group.Key,
                group => group.Select(drop =>
                    GraphItem.GetOrCreate(drop.Item2)).Distinct().ToList()
            ).ToList();
        Logger.LogInfo($"merged to {mergedDrops.Count} unique drops");
        mergedDrops.ForEach(drop => Drops.Add(drop.Key, drop.Value));
    }
}