using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data.Drops;

public class MineRock5DropInitializer: DropsInitializerDropData<MineRock5>
{
    protected override Dictionary<string, MineRock5> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(MineRock5))
            .ToDictionary(kv => kv.Key, kv => (MineRock5)kv.Value);
    }

    protected override List<DropTable.DropData> GetDropList(MineRock5 input)
    {
        if (input.m_dropItems != null) return FromTable(input.m_dropItems);
        Logger.LogWarning($"'m_dropItems' not found for {typeof(MineRock5)}");
        return new List<DropTable.DropData>();
    }
}