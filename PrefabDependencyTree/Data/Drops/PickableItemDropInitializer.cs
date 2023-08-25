using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace PrefabDependencyTree.Data.Drops;

public class PickableItemDropInitializer : DropsInitializer<PickableItem, ItemDrop>
{
    protected override Dictionary<string, PickableItem> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(PickableItem))
            .ToDictionary(kv => kv.Key, kv => (PickableItem)kv.Value);
    }

    protected override List<ItemDrop> GetDropList(PickableItem input)
    {
        return new List<ItemDrop> { input.m_itemPrefab };
    }

    protected override GameObject GetObject(ItemDrop input)
    {
        return input.gameObject;
    }
}