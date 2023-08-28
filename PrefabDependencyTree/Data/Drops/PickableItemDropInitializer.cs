using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using UnityEngine;

namespace PrefabDependencyTree.Data.Drops;

public class PickableItemDropInitializer : DropsInitializer<PickableItem, ItemDrop>
{
    protected override Dictionary<Tuple<string, DropType>, PickableItem> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(PickableItem)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Pickable),
            kv => (PickableItem)kv.Value
        );
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