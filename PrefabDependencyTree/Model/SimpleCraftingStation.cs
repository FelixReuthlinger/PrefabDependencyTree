using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PrefabDependencyTree.Model;

public class SimpleCraftingStation
{
    [UsedImplicitly] public string StationName;
    [UsedImplicitly] public List<string> ExtensionNames;

    [UsedImplicitly]
    public static Dictionary<string, SimpleCraftingStation> FromStationExtensions(List<StationExtension> extensions)
    {
        return extensions
            .Select(extension => Tuple.Create(extension.m_craftingStation.name, extension.name))
            .GroupBy(kv => kv.Item1)
            .ToDictionary(
                group => group.Key,
                group => new SimpleCraftingStation
                {
                    StationName = group.Key,
                    ExtensionNames = group.Select(grouped => grouped.Item2).ToList()
                }
            );
    }
}