using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PrefabDependencyTree.Data.Drops.Generic;
using UnityEngine;

namespace PrefabDependencyTree.Data.Drops;

public class CharacterDropInitializer : DropsInitializer<CharacterDrop, CharacterDrop.Drop>
{
    protected override Dictionary<Tuple<string, DropType>, CharacterDrop> GetGameObjects()
    {
        return PrefabManager.Cache.GetPrefabs(typeof(Character)).ToDictionary(
            kv => Tuple.Create(kv.Key, DropType.Character),
            kv => (CharacterDrop)((Character)kv.Value).GetComponent(typeof(CharacterDrop))
        );
    }

    protected override List<CharacterDrop.Drop> GetDropList(CharacterDrop input)
    {
        if (input != null && input.m_drops != null) return input.m_drops.ToList();
        return new List<CharacterDrop.Drop>();
    }

    protected override GameObject GetObject(CharacterDrop.Drop input)
    {
        return input.m_prefab;
    }
}