using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph.Core;
using PrefabDependencyTree.Graph;

namespace PrefabDependencyTree.Model;

public class GraphCraftingStation : BaseCrafting
{
    private readonly List<string> ExtensionNames;

    private GraphCraftingStation(string stationName, List<string> extensionNames) : base(stationName)
    {
        ExtensionNames = extensionNames;
    }

    public static Dictionary<string, GraphCraftingStation> FromExtensionsAndStations(
        List<StationExtension> extensions,
        List<CraftingStation> stations)
    {
        Dictionary<string, GraphCraftingStation> stationsFromExtensions = extensions
            .Select(extension => Tuple.Create(extension.m_craftingStation.name, extension.name))
            .GroupBy(tuple => tuple.Item1)
            .ToDictionary(
                group => group.Key,
                group => new GraphCraftingStation(
                    stationName: group.Key,
                    extensionNames: group.Select(tuple => tuple.Item2).ToList())
            );
        return stations.ToDictionary(
            station => station.name,
            station =>
            {
                if (stationsFromExtensions.TryGetValue(station.name, out GraphCraftingStation stationFromExtension))
                    return stationFromExtension;
                return new GraphCraftingStation(station.name, new List<string>());
            });
    }

    public void CreateStationGraph()
    {
        DotNode stationNode = GraphBuilder.GetOrCreateNode(Name);

        foreach (string extensionName in ExtensionNames)
        {
            var extensionNode = GraphBuilder.GetOrCreateNode(extensionName);
            GraphBuilder.GetOrCreateEdge(extensionNode, stationNode);
        }

        foreach (KeyValuePair<string, GraphRecipe> recipe in Recipes)
        {
            DotNode recipeNode = recipe.Value.AddToGraph(GraphBuilder.Graph);
            GraphBuilder.GetOrCreateEdge(stationNode, recipeNode);
        }
    }
}