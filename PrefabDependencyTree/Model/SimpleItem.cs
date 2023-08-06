using JetBrains.Annotations;

namespace PrefabDependencyTree.Model;

public class SimpleItem
{
    [UsedImplicitly] public string ItemName;

    [UsedImplicitly]
    public static SimpleItem FromItemDrop(ItemDrop original)
    {
        return new SimpleItem
        {
            ItemName = original.name
        };
    }
}