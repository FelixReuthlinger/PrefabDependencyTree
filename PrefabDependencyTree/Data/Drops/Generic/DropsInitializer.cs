using System;
using System.Collections.Generic;
using System.Linq;
using PrefabDependencyTree.Model;
using UnityEngine;
using Logger = PrefabDependencyTree.Util.Logger;

namespace PrefabDependencyTree.Data.Drops.Generic;

public abstract class DropsInitializer<T, U> : Initializer
{
    protected abstract Dictionary<Tuple<string, DropType>, T> GetGameObjects();

    protected abstract List<U> GetDropList(T input);

    protected abstract GameObject GetObject(U input);

    public void InitializeDrops()
    {
        int countBefore = DataHarvester.Drops.Count;
        foreach (KeyValuePair<Tuple<string, DropType>, T> pair in GetGameObjects())
        {
            List<U> dropData = GetDropList(pair.Value).Where(drop => drop != null).ToList();
            if (dropData.Count <= 0)
            {
                Logger.LogInfo($"{typeof(T)} '{pair.Key}' does not have drop data - skipping");
                continue;
            }

            List<GraphItem> newDrops = DataHarvester.Drops.TryGetValue(pair.Key, out List<GraphItem> drops)
                ? drops
                : new List<GraphItem>();
            foreach (U drop in dropData)
            {
                if (drop == null)
                {
                    Logger.LogInfo(
                        $"{typeof(T)} - {typeof(U)}: found null drop in drop data (length {dropData.Count})");
                    continue;
                }

                switch (drop)
                {
                    case ItemDrop itemDrop:
                        newDrops.Add(GraphItem.GetOrCreate(itemDrop));
                        break;
                    default:
                        var newDrop = GraphItem.GetOrCreate(GetObject(drop));
                        if(newDrop != null) newDrops.Add(newDrop);
                        break;
                }
            }

            DataHarvester.Drops[Tuple.Create(pair.Key.Item1,pair.Key.Item2)] = newDrops.Distinct().ToList();
        }

        Logger.LogInfo(
            $"loaded {DataHarvester.Drops.Count - countBefore} drops from {typeof(T)} ({typeof(U)}) from game");
    }
}