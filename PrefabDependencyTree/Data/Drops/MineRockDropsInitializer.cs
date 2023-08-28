using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class MineRockDropsInitializer : DropsInitializerDropData<MineRock>
{
    protected override Dictionary<Tuple<string, DropType>, MineRock> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(MineRock)).ToDictionary(
            kv => Tuple.Create(kv.Key,DropType.MineRock),
            kv => (MineRock)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(MineRock input)
    {
        if (input.m_dropItems != null) return FromTable(input.m_dropItems);
        Logger.LogWarning($"'m_dropItems' not found for {typeof(MineRock)}");
        return new List<DropTable.DropData>();
    }
}