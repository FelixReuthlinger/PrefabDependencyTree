using System;
using System.Collections.Generic;
using System.Linq;

namespace PrefabDependencyTree.Model;

public class GraphRecipe
{
    public string RecipeName;
    public Tuple<GraphItem, int> CraftedItem;
    public Dictionary<GraphItem, int> RequiredItems;

    private GraphRecipe()
    {
    }

    public static GraphRecipe FromRecipe(Recipe fromGame)
    {
        if (fromGame.m_item == null)
            throw new ArgumentException($"recipe '{fromGame.name}' does not produce an item");
        if (!fromGame.m_resources.ToList().TrueForAll(recourse => recourse.m_resItem != null))
            throw new ArgumentException($"recipe '{fromGame.name}' contains a null required resource");
        return new GraphRecipe
        {
            RecipeName = fromGame.name,
            CraftedItem = Tuple.Create(GraphItem.GetOrCreate(fromGame.m_item), 1),
            RequiredItems = fromGame.m_resources.ToDictionary(
                requirement => GraphItem.GetOrCreate(requirement.m_resItem),
                requirement => requirement.m_amount
            )
        };
    }

    public static Dictionary<string, GraphRecipe> FromSmelter(Smelter smelter)
    {
        var fuel = smelter.m_fuelItem == null
            ? "Air" // kiln does not use fuel 
            : smelter.m_fuelItem.name;

        return smelter.m_conversion.Select(conversion =>
            new GraphRecipe
            {
                RecipeName = GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                CraftedItem = Tuple.Create(GraphItem.GetOrCreate(conversion.m_to), 1),
                RequiredItems = new Dictionary<GraphItem, int>
                {
                    {
                        GraphItem.GetOrCreate(fuel, ItemDrop.ItemData.ItemType.Material.ToString()),
                        smelter.m_fuelPerProduct
                    },
                    { GraphItem.GetOrCreate(conversion.m_from), 1 }
                }
            }
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static Dictionary<string, GraphRecipe> FromIncinerator(Incinerator incinerator)
    {
        return incinerator.m_conversions.Select(
                conversion => new GraphRecipe
                {
                    RecipeName = GetProcessorRecipeName(
                        "Incinerator_Sacrifice",
                        conversion.m_result.name
                    ),
                    CraftedItem = Tuple.Create(GraphItem.GetOrCreate(conversion.m_result), conversion.m_resultAmount),
                    RequiredItems = conversion.m_requirements.ToDictionary(
                        requirement => GraphItem.GetOrCreate(requirement.m_resItem),
                        requirement => requirement.m_amount
                    )
                }
            )
            .GroupBy(recipe => recipe.RecipeName)
            .ToDictionary(
                group => group.Key,
                group => new GraphRecipe
                {
                    RecipeName = group.Key,
                    CraftedItem = group.First().CraftedItem,
                    RequiredItems = group
                        .SelectMany(recipe => recipe.RequiredItems)
                        .ToDictionary(
                            tuple => tuple.Key,
                            tuple => tuple.Value
                        )
                }
            );
    }

    public static Dictionary<string, GraphRecipe> FromFermenter(Fermenter fermenter)
    {
        return fermenter.m_conversion.Select(
            conversion => new GraphRecipe
            {
                RecipeName = GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                CraftedItem = Tuple.Create(GraphItem.GetOrCreate(conversion.m_to), conversion.m_producedItems),
                RequiredItems = new Dictionary<GraphItem, int> { { GraphItem.GetOrCreate(conversion.m_from), 1 } }
            }
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static Dictionary<string, GraphRecipe> FromCookingStation(CookingStation cookingStation)
    {
        var fuel = cookingStation.m_fuelItem == null ? "Fire" : cookingStation.m_fuelItem.name;
        return cookingStation.m_conversion.Select(
            conversion => new GraphRecipe
            {
                RecipeName = GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                CraftedItem = Tuple.Create(GraphItem.GetOrCreate(conversion.m_to), 1),
                RequiredItems = new Dictionary<GraphItem, int>
                {
                    { GraphItem.GetOrCreate(conversion.m_from), 1 },
                    { GraphItem.GetOrCreate(fuel, ItemDrop.ItemData.ItemType.Material.ToString()), 1 }
                }
            }
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    private static string GetProcessorRecipeName(string fromItem, string toItem)
    {
        return $"[processing {fromItem} to {toItem}]";
    }

    public override string ToString()
    {
        var requirements = string.Join("\n    ",
            RequiredItems.Select(requiredItem => $"[{requiredItem.Key} x{requiredItem.Value}]")
        );
        return $"[recipe '{RecipeName}' to create {CraftedItem.Item1} requires:\n" +
               $"    {requirements}\n" +
               $"]";
    }
}