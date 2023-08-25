using System.Collections.Generic;
using UnityEngine;
using Logger = PrefabDependencyTree.Util.Logger;

namespace PrefabDependencyTree.Data.Drops;

public abstract class DropsInitializerDropData<T> : DropsInitializer<T, DropTable.DropData>
{
    protected override GameObject GetObject(DropTable.DropData input)
    {
        return input.m_item;
    }

    protected List<DropTable.DropData> FromTable(DropTable table)
    {
        if (table != null) return table.m_drops;
        Logger.LogWarning($"missing drop table for type '{typeof(T)}'");
        return new List<DropTable.DropData>();
    }
}