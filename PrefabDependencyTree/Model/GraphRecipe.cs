using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Jotunn;
using PrefabDependencyTree.Graph;

namespace PrefabDependencyTree.Model;

public class GraphRecipe
{
    private readonly string RecipeName;
    private readonly Tuple<GraphItem, int> CraftedItem;
    private readonly Dictionary<GraphItem, int> RequiredItems;

    private GraphRecipe(string recipeName, Tuple<string, int> craftedItem, Dictionary<string, int> requiredIngredients)
    {
        RecipeName = recipeName;
        CraftedItem = Tuple.Create(GraphItem.GetOrCreate(craftedItem.Item1), craftedItem.Item2);
        RequiredItems = requiredIngredients.ToDictionary(
            ingredient => GraphItem.GetOrCreate(ingredient.Key),
            ingredient => ingredient.Value
        );
    }

    public static GraphRecipe FromRecipe(Recipe fromGame)
    {
        if (fromGame.m_item == null)
            throw new ArgumentException($"recipe '{fromGame.name}' does not produce an item");
        if (!fromGame.m_resources.ToList().TrueForAll(recourse => recourse.m_resItem != null))
            throw new ArgumentException($"recipe '{fromGame.name}' contains a null required resource");
        return new GraphRecipe(
            recipeName: fromGame.name,
            craftedItem: Tuple.Create(fromGame.m_item.name, 1),
            requiredIngredients: fromGame.m_resources.ToDictionary(
                requirement => requirement.m_resItem.name,
                requirement => requirement.m_amount)
        );
    }

    public static Dictionary<string, GraphRecipe> FromSmelter(Smelter smelter)
    {
        var fuel = smelter.m_fuelItem == null
            ? "Air" // kiln does not use fuel 
            : smelter.m_fuelItem.name;

        return smelter.m_conversion.Select(conversion =>
            new GraphRecipe(
                recipeName: GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                craftedItem: Tuple.Create(conversion.m_to.name, 1),
                requiredIngredients: new Dictionary<string, int>
                {
                    { fuel, smelter.m_fuelPerProduct },
                    { conversion.m_from.name, 1 }
                }
            )
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static Dictionary<string, GraphRecipe> FromIncinerator(Incinerator incinerator)
    {
        return incinerator.m_conversions.Select(
            conversion => new GraphRecipe(
                recipeName: GetProcessorRecipeName(
                    string.Join("+", conversion.m_requirements.Select(requirement => requirement.m_resItem.name)),
                    conversion.m_result.name
                ),
                craftedItem: Tuple.Create(conversion.m_result.name, conversion.m_resultAmount),
                requiredIngredients: conversion.m_requirements.ToDictionary(
                    requirement => requirement.m_resItem.name,
                    requirement => requirement.m_amount
                )
            )
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static Dictionary<string, GraphRecipe> FromFermenter(Fermenter fermenter)
    {
        return fermenter.m_conversion.Select(
            conversion => new GraphRecipe(
                recipeName: GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                craftedItem: Tuple.Create(conversion.m_to.name, conversion.m_producedItems),
                requiredIngredients: new Dictionary<string, int> { { conversion.m_from.name, 1 } }
            )
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static Dictionary<string, GraphRecipe> FromCookingStation(CookingStation cookingStation)
    {
        var fuel = cookingStation.m_fuelItem == null ? "Fire" : cookingStation.m_fuelItem.name;
        return cookingStation.m_conversion.Select(
            conversion => new GraphRecipe(
                recipeName: GetProcessorRecipeName(conversion.m_from.name, conversion.m_to.name),
                craftedItem: Tuple.Create(conversion.m_to.name, 1),
                requiredIngredients: new Dictionary<string, int>
                {
                    { conversion.m_from.name, 1 },
                    { fuel, 1 }
                }
            )
        ).ToDictionary(recipe => recipe.RecipeName, recipe => recipe);
    }

    public static string GetProcessorRecipeName(string fromItem, string toItem)
    {
        return $"[processing '{fromItem}' to '{toItem}']";
    }

    public DotNode AddToGraph(DotBaseGraph graph)
    {
        Logger.LogInfo($"processing recipe {this}");
        var recipeNode = GraphBuilder.GetOrCreateNode(RecipeName);
        graph.Add(recipeNode);

        Logger.LogDebug($"{RecipeName} -> {CraftedItem.Item1.ItemName}");
        var craftedItemNode = GraphBuilder.GetOrCreateNode(CraftedItem.Item1.ItemName);
        graph.Add(craftedItemNode);

        var createdEdge = GraphBuilder.GetOrCreateEdge(RecipeName, CraftedItem.Item1.ItemName);
        graph.Add(createdEdge);

        foreach (KeyValuePair<GraphItem, int> requiredItem in RequiredItems)
        {
            var requiredItemNode = GraphBuilder.GetOrCreateNode(requiredItem.Key.ItemName);
            graph.Add(requiredItemNode);

            Logger.LogDebug($"{requiredItem.Key.ItemName} -> {RecipeName}");
            var requirementEdge = GraphBuilder.GetOrCreateEdge(requiredItem.Key.ItemName, RecipeName);
            graph.Add(requirementEdge);
        }

        return recipeNode;
    }

    public override string ToString()
    {
        var requirements = string.Join(",",
            RequiredItems
                .Select(requiredItem => $"{requiredItem.Key} required x{requiredItem.Value}")
                .ToArray()
        );
        return $"recipe '{RecipeName}' to create '{CraftedItem.Item1}' requires '{requirements}'";
    }
}