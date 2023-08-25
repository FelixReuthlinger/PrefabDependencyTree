using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using Logger = PrefabDependencyTree.Util.Logger;

namespace PrefabDependencyTree.Data.Drops;

public class PickableItemRandomDropInitializer : DropsInitializer<PickableItem, PickableItem.RandomItem>
{
    protected override Dictionary<string, PickableItem> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(PickableItem))
            .ToDictionary(kv => kv.Key, kv => (PickableItem)kv.Value);
    }

    protected override List<PickableItem.RandomItem> GetDropList(PickableItem input)
    {
        if (input.m_randomItemPrefabs != null) return input.m_randomItemPrefabs.ToList();
        Logger.LogWarning($"'m_randomItemPrefabs' not found for {typeof(PickableItem)}");
        return new List<PickableItem.RandomItem>();
    }

    protected override GameObject GetObject(PickableItem.RandomItem input)
    {
        return input.m_itemPrefab.gameObject;
    }
}