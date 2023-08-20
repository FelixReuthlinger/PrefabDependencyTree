using PrefabDependencyTree.Data;

namespace PrefabDependencyTree.Model;

public class GraphItem
{
    public string ItemName { get; private set; }

    private GraphItem()
    {
    }

    public static GraphItem GetOrCreate(string itemName)
    {
        if (DataHarvester.Items.TryGetValue(itemName, out GraphItem found))
            return found;
        var newItem = new GraphItem { ItemName = itemName };
        DataHarvester.Items.Add(itemName, newItem);
        return newItem;
    }

    public override string ToString()
    {
        return ItemName;
    }
}