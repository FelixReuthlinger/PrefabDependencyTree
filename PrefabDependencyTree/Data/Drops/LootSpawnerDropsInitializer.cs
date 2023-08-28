using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class LootSpawnerDropsInitializer : DropsInitializerDropData<LootSpawner>
{
    protected override Dictionary<Tuple<string, DropType>, LootSpawner> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(LootSpawner)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.LootSpawner),
            kv => (LootSpawner)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(LootSpawner input)
    {
        if (input.m_items != null) return FromTable(input.m_items);
        Logger.LogWarning($"'m_items' not found for {typeof(LootSpawner)}");
        return new List<DropTable.DropData>();
    }
}