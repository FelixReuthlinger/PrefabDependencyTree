using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class ContainerDropsInitializer : DropsInitializerDropData<Container>
{
    protected override Dictionary<string, Container> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(Container))
            .ToDictionary(kv => kv.Key, kv => (Container)kv.Value);
    }

    protected override List<DropTable.DropData> GetDropList(Container input)
    {
        if (input.m_defaultItems != null) return FromTable(input.m_defaultItems);
        Logger.LogWarning($"'m_defaultItems' not found for {typeof(Container)}");
        return new List<DropTable.DropData>();
    }
}