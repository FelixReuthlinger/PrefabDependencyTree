using JetBrains.Annotations;

namespace PrefabDependencyTree.Model;

public class StationRequirement
{
    [UsedImplicitly] public string stationName;
    [UsedImplicitly] public int minStationLevel;
}