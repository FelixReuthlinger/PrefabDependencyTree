using System.Collections.Generic;

namespace PrefabDependencyTree.Data.Filters;

public class UnfilteredData : FilteredData
{
    public UnfilteredData()
    {
        FilterType = FilterType.Unfiltered;
        ItemTypeFilters = new List<string>();

        Items = DataHarvester.Items;
        UnboundRecipes = DataHarvester.UnboundRecipes;
        CraftingStations = DataHarvester.CraftingStations;
        Processors = DataHarvester.Processors;
        Drops = DataHarvester.Drops;
        Pieces = DataHarvester.Pieces;
    }}