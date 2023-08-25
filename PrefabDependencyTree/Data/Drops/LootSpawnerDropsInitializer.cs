using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class LootSpawnerDropsInitializer : DropsInitializerDropData<LootSpawner>
{
    protected override Dictionary<string, LootSpawner> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(LootSpawner))
            .ToDictionary(kv => kv.Key, kv => (LootSpawner)kv.Value);
    }

    protected override List<DropTable.DropData> GetDropList(LootSpawner input)
    {
        if (input.m_items != null) return FromTable(input.m_items);
        Logger.LogWarning($"'m_items' not found for {typeof(LootSpawner)}");
        return new List<DropTable.DropData>();
    }
}