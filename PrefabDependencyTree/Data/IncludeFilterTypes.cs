using System.Collections.Generic;
using System.Linq;

namespace PrefabDependencyTree.Data;

public class IncludeFilterTypes: FilteredData
{
    public IncludeFilterTypes(List<string> itemTypeFilters)
    {
        ItemTypeFilters = itemTypeFilters;
        FilterType = FilterType.Include;

        Items = DataHarvester.Items
            .Where(item => itemTypeFilters.Contains(item.Value.ItemType))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        UnboundRecipes = DataHarvester.UnboundRecipes
            .Where(recipe =>
                itemTypeFilters.Contains(recipe.Value.CraftedItem.Item1.ItemType)
                || recipe.Value.RequiredItems.Any(requiredItem =>
                    itemTypeFilters.Contains(requiredItem.Key.ItemType)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        CraftingStations = DataHarvester.CraftingStations
            .Where(station => station.Value.ContainsItemTypes(itemTypeFilters))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        Processors = DataHarvester.Processors
            .Where(processor => processor.Value.ContainsItemTypes(itemTypeFilters))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        Drops = DataHarvester.Drops
            .Where(drop => drop.Value.Any(item => itemTypeFilters.Contains(item.ItemType)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        Pieces = DataHarvester.Pieces
            .Where(piece =>
                piece.Value.BuildRequirements.Any(requirement =>
                    itemTypeFilters.Contains(requirement.Key.ItemType)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}