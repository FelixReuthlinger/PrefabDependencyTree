using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PrefabDependencyTree.Model;

public class IngredientsRequirement
{
    [UsedImplicitly] public SimpleItem Item;
    [UsedImplicitly] public int ItemAmount;

    public static List<IngredientsRequirement> FromRequirements(Piece.Requirement[] original)
    {
        return original
            .Where(req => req != null)
            .Where(req => req.m_resItem != null)
            .Select(req => new IngredientsRequirement
            {
                Item = SimpleItem.FromItemDrop(req.m_resItem),
                ItemAmount = req.m_amount
            })
            .ToList();
    }
}