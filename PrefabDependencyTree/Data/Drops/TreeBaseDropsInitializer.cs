using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class TreeBaseDropsInitializer : DropsInitializerDropData<TreeBase>
{
    protected override Dictionary<Tuple<string, DropType>, TreeBase> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(TreeBase)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Tree),
            kv => (TreeBase)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(TreeBase input)
    {
        if (input.m_dropWhenDestroyed != null) return FromTable(input.m_dropWhenDestroyed);
        Logger.LogWarning($"'m_dropWhenDestroyed' not found for {typeof(TreeBase)}");
        return new List<DropTable.DropData>();
    }
}