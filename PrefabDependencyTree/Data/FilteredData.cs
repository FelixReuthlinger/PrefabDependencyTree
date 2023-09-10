using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph.Core;
using PrefabDependencyTree.Model;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data;

public abstract class FilteredData
{
    protected Dictionary<string, GraphItem> Items;
    protected Dictionary<string, GraphRecipe> UnboundRecipes;
    protected Dictionary<string, GraphCraftingStation> CraftingStations;
    protected Dictionary<string, GraphProcessor> Processors;
    protected Dictionary<Tuple<string, DropType>, List<GraphItem>> Drops;
    protected Dictionary<string, GraphPiece> Pieces;

    protected List<string> ItemTypeFilters;
    protected FilterType FilterType;

    public void LogOverview()
    {
        Logger.LogInfo("vvvv filtered data overview vvvv");
        Logger.LogInfo($"  applied {FilterType.ToString()} filters: {string.Join(", ", ItemTypeFilters)}");
        Logger.LogInfo($"    total {Items.Count} items registered");
        Logger.LogInfo($"    total {Drops.Count} drops registered");
        Logger.LogInfo($"    total {Pieces.Count} pieces registered");
        Logger.LogInfo($"    total {CraftingStations.Count} crafting stations registered");
        Logger.LogInfo($"    total {Processors.Count} processor stations (fermenter, smelter, ...) registered");
        Logger.LogInfo($"    total {UnboundRecipes.Count} unbound recipes registered");
        Logger.LogInfo("^^^^ filtered data overview ^^^^");
    }

    public DotGraph CreateGraph()
    {
        var graphBuilder = new GraphBuilder();

        Items.ToList().ForEach(item => graphBuilder.AddNode(item.Value.ItemName, item.Value.ItemType));

        foreach (KeyValuePair<Tuple<string, DropType>, List<GraphItem>> drop in Drops)
        {
            graphBuilder.AddNode(drop.Key.Item1, drop.Key.Item2.ToString());
            foreach (GraphItem item in drop.Value)
            {
                graphBuilder.AddNode(item);
                graphBuilder.AddEdge(drop.Key.Item1, item.ItemName);
            }
        }

        foreach (KeyValuePair<string, GraphPiece> piece in Pieces)
        {
            graphBuilder.AddNode(piece.Value.PieceName, NodeTypes.Piece.ToString());
            graphBuilder.AddNode(piece.Value.RequiredCraftingStation, NodeTypes.CraftingStation.ToString());
            graphBuilder.AddEdge(piece.Value.RequiredCraftingStation, piece.Value.PieceName);
            foreach (KeyValuePair<GraphItem, int> requirement in piece.Value.BuildRequirements)
            {
                graphBuilder.AddNode(requirement.Key);
                graphBuilder.AddEdge(requirement.Key.ItemName, piece.Value.PieceName);
            }
        }

        AddRecipesToGraph(graphBuilder, UnboundRecipes);

        foreach (KeyValuePair<string, GraphCraftingStation> station in CraftingStations)
        {
            graphBuilder.AddNode(station.Value.Name, NodeTypes.CraftingStation.ToString());
            foreach (string extensionName in station.Value.ExtensionNames)
            {
                graphBuilder.AddNode(extensionName, NodeTypes.CraftingStation.ToString());
                graphBuilder.AddEdge(extensionName, station.Value.Name);
            }

            AddRecipesToGraph(graphBuilder, station.Value.Recipes);
        }

        foreach (KeyValuePair<string, GraphProcessor> processor in Processors)
        {
            graphBuilder.AddNode(processor.Value.Name, NodeTypes.Processor.ToString());
            AddRecipesToGraph(graphBuilder, processor.Value.Recipes);
        }

        return graphBuilder.BuildGraph();
    }

    private static void AddRecipesToGraph(GraphBuilder graphBuilder, Dictionary<string, GraphRecipe> recipes)
    {
        foreach (KeyValuePair<string, GraphRecipe> recipe in recipes)
        {
            graphBuilder.AddNode(recipe.Value.RecipeName, NodeTypes.Recipe.ToString());
            graphBuilder.AddNode(recipe.Value.CraftedItem.Item1);
            graphBuilder.AddEdge(recipe.Value.RecipeName, recipe.Value.CraftedItem.Item1.ItemName);
            foreach (KeyValuePair<GraphItem, int> item in recipe.Value.RequiredItems)
            {
                graphBuilder.AddNode(item.Key);
                graphBuilder.AddEdge(item.Key.ItemName, recipe.Value.RecipeName);
            }
        }
    }
}