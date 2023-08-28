using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using UnityEngine;

namespace PrefabDependencyTree.Data.Drops;

public class PickableDropInitializer : DropsInitializer<Pickable, GameObject>
{
    protected override Dictionary<Tuple<string, DropType>, Pickable> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(Pickable)).ToDictionary(
            kv => Tuple.Create(kv.Key,DropType.Pickable),
            kv => (Pickable)kv.Value
        );
    }

    protected override List<GameObject> GetDropList(Pickable input)
    {
        return new List<GameObject> { input.m_itemPrefab };
    }

    protected override GameObject GetObject(GameObject input)
    {
        return input;
    }
}