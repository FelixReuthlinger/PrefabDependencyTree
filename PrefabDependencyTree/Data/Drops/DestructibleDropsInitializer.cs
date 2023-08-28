using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class DestructibleDropsInitializer : DropsInitializerDropData<DropOnDestroyed>
{
    protected override Dictionary<Tuple<string, DropType>, DropOnDestroyed> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(DropOnDestroyed)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Destructible),
            kv => (DropOnDestroyed)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(DropOnDestroyed input)
    {
        if (input.m_dropWhenDestroyed != null) return FromTable(input.m_dropWhenDestroyed);
        Logger.LogWarning($"'m_dropWhenDestroyed' not found for {typeof(DropOnDestroyed)}");
        return new List<DropTable.DropData>();
    }
}