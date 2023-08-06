using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PrefabDependencyTree.Model;

public class SimpleRecipe
{
    [UsedImplicitly] [CanBeNull] public StationRequirement Station;
    [UsedImplicitly] public List<IngredientsRequirement> Ingredients;
    [UsedImplicitly] public SimpleItem Item;

    public string RecipeName()
    {
        string ingredientsText = String.Join(",",
            Ingredients.Select(ingredient => 
                ingredient.ItemAmount + "x" + ingredient.Item.ItemName));
        return $"'{Item.ItemName}' from '{ingredientsText}'";
    }

    [UsedImplicitly]
    public static SimpleRecipe FromRecipe(Recipe original)
    {
        List<IngredientsRequirement> ingredients = IngredientsRequirement.FromRequirements(original.m_resources);
        StationRequirement station = original.m_craftingStation != null
            ? new StationRequirement
            {
                stationName = original.m_craftingStation.name,
                minStationLevel = original.m_minStationLevel
            }
            : null;
        return new SimpleRecipe
        {
            Station = station,
            Ingredients = ingredients,
            Item = SimpleItem.FromItemDrop(original.m_item)
        };
    }
}