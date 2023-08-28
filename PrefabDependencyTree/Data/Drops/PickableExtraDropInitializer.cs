﻿using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class PickableExtraDropInitializer : DropsInitializerDropData<Pickable>
{
    protected override Dictionary<string, Pickable> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(Pickable))
            .ToDictionary(kv => kv.Key, kv => (Pickable)kv.Value);
    }

    protected override List<DropTable.DropData> GetDropList(Pickable input)
    {
        if (input.m_extraDrops != null) return FromTable(input.m_extraDrops);
        Logger.LogWarning($"'m_extraDrops' not found for {typeof(Pickable)}");
        return new List<DropTable.DropData>();
    }
}