using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class ContainerDropsInitializer : DropsInitializerDropData<Container>
{
    protected override Dictionary<Tuple<string, DropType>, Container> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(Container)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Container),
            kv => (Container)kv.Value
        );
    }

    protected override List<DropTable.DropData> GetDropList(Container input)
    {
        if (input.m_defaultItems != null) return FromTable(input.m_defaultItems);
        Logger.LogWarning($"'m_defaultItems' not found for {typeof(Container)}");
        return new List<DropTable.DropData>();
    }
}