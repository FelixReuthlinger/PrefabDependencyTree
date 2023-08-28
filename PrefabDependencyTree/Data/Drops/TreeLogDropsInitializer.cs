using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class TreeLogDropsInitializer : DropsInitializerDropData<TreeLog>
{
    protected override Dictionary<Tuple<string, DropType>, TreeLog> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(TreeLog)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Tree),
            kv => (TreeLog)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(TreeLog input)
    {
        if (input.m_dropWhenDestroyed != null) return FromTable(input.m_dropWhenDestroyed);
        Logger.LogWarning($"'m_dropWhenDestroyed' not found for {typeof(TreeLog)}");
        return new List<DropTable.DropData>();
    }
}