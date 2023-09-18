using System.Collections.Generic;
using System.Linq;
using PrefabDependencyTree.Model;

namespace PrefabDependencyTree.Data.Filters;

public class ExcludeFilterTypes : FilteredData
{
    public ExcludeFilterTypes(List<string> itemTypeFilters)
    {
        ItemTypeFilters = itemTypeFilters;
        FilterType = FilterType.Exclude;

        Items = DataHarvester.Items
            .Where(item => !itemTypeFilters.Contains(item.Value.ItemType))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        UnboundRecipes = DataHarvester.UnboundRecipes
            .Where(recipe =>
                !itemTypeFilters.Contains(recipe.Value.CraftedItem.Item1.ItemType)
                && !recipe.Value.RequiredItems.Any(
                    requiredItem => itemTypeFilters.Contains(requiredItem.Key.ItemType))
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        CraftingStations = DataHarvester.CraftingStations.ToDictionary(
            station => station.Key,
            station =>
            {
                station.Value.RemoveRecipesForTypes(itemTypeFilters);
                return station.Value;
            })
            .Where(station => station.Value.Recipes.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        Processors = DataHarvester.Processors
            .ToDictionary(
                processor => processor.Key,
                processor =>
                {
                    processor.Value.RemoveRecipesForTypes(itemTypeFilters);
                    return processor.Value;
                })
            .Where(processor => processor.Value.Recipes.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        Drops = DataHarvester.Drops
            .ToDictionary(
                drop => drop.Key,
                drop =>
                {
                    List<GraphItem> reducedDrops = drop.Value
                        .Where(item => !itemTypeFilters.Contains(item.ItemType)).ToList();
                    drop.Value.Clear();
                    drop.Value.AddRange(reducedDrops);
                    return drop.Value;
                })
            .Where(drop => drop.Value.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        Pieces = DataHarvester.Pieces
            .Where(piece =>
                !piece.Value.BuildRequirements.Any(requirement =>
                    itemTypeFilters.Contains(requirement.Key.ItemType)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}