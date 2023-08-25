using JetBrains.Annotations;
using PrefabDependencyTree.Data;
using UnityEngine;

namespace PrefabDependencyTree.Model;

public class GraphItem
{
    public string ItemName { get; private set; }
    public string ItemType { get; set; }

    private GraphItem()
    {
    }

    public static GraphItem GetOrCreate(string itemName, string itemType)
    {
        if (DataHarvester.Items.TryGetValue(itemName, out GraphItem found))
            return found;
        var newItem = new GraphItem { ItemName = itemName, ItemType = itemType };
        DataHarvester.Items.Add(itemName, newItem);
        return newItem;
    }

    public static GraphItem GetOrCreate(ItemDrop drop)
    {
        return GetOrCreate(drop.name, drop.m_itemData.m_shared.m_itemType.ToString());
    }

    [CanBeNull]
    public static GraphItem GetOrCreate(GameObject gameObject)
    {
        return gameObject.TryGetComponent(out ItemDrop drop)
            ? GetOrCreate(drop.name, drop.m_itemData.m_shared.m_itemType.ToString())
            : null;
    }

    public override string ToString()
    {
        return $"[item name '{ItemName}', type '{ItemType}']";
    }
}