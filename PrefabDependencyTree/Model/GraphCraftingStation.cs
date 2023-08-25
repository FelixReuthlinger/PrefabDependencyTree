using System;
using System.Collections.Generic;
using System.Linq;

namespace PrefabDependencyTree.Model;

public class GraphCraftingStation : BaseCrafting
{
    public readonly List<string> ExtensionNames;

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

    public override string ToString()
    {
        if (ExtensionNames.Count <= 0)
            return $"[crafting station {base.ToString()}" +
                   $"\n]";
        string extensions = string.Join(", ", ExtensionNames);
        return $"[crafting station {base.ToString()}\n" +
               $"  has extensions: {extensions}\n" +
               $"]";
    }
}