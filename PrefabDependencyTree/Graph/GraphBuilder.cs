using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Jotunn;
using PrefabDependencyTree.Data;
using PrefabDependencyTree.Model;

namespace PrefabDependencyTree.Graph;

public static class GraphBuilder
{
    public static readonly DotGraph Graph = new DotGraph().WithIdentifier("graph").Directed();
    private static readonly Dictionary<string, DotNode> Nodes = new();
    private static readonly Dictionary<string, DotEdge> Edges = new();

    public static DotGraph CreateGraph()
    {
        // create all drops from everywhere
        foreach (KeyValuePair<string,List<GraphItem>> drop in DataHarvester.Drops)
        {
            var droppedFromNode = GetOrCreateNode(drop.Key);
            foreach (var droppedItemNode in drop.Value.Select(item => GetOrCreateNode(item.ItemName)))
            {
                GetOrCreateEdge(droppedFromNode, droppedItemNode);
            }
        }
        
        // create all pieces
        foreach (KeyValuePair<string,GraphPiece> piece in DataHarvester.Pieces)
        {
            var pieceNode = GetOrCreateNode(piece.Value.PieceName);
            var craftingStation = GetOrCreateNode(piece.Value.RequiredCraftingStation);
            GetOrCreateEdge(craftingStation, pieceNode);
            foreach (var requiredItem in piece.Value.BuildRequirements
                         .Select(requirement => GetOrCreateNode(requirement.Key.ItemName)))
            {
                GetOrCreateEdge(requiredItem, pieceNode);
            }
        }
        
        // first we add all free recipes
        foreach (KeyValuePair<string,GraphRecipe> recipe in DataHarvester.UnboundRecipes)
        {
            recipe.Value.AddToGraph(Graph);
        }
        
        // process all found stations and smelters to graph objects
        foreach (KeyValuePair<string,GraphCraftingStation> station in DataHarvester.CraftingStations)
        {
            station.Value.CreateStationGraph();
        }
        foreach (KeyValuePair<string,GraphProcessor> smelter in DataHarvester.Processors)
        {
            smelter.Value.CreateProcessorGraph();
        }
        
        // build the graph with all stations and smelters
        foreach (KeyValuePair<string,DotNode> node in Nodes)
        {
            Graph.Add(node.Value);
        }
        foreach (KeyValuePair<string,DotEdge> edge in Edges)
        {
            Graph.Add(edge.Value);
        }
        
        return Graph;
    }

    public static DotNode GetOrCreateNode(string name)
    {
        DotNode node;
        if (Nodes.TryGetValue(name, out DotNode existingStationGraph))
        {
            node = existingStationGraph;
        }
        else
        {
            node = new DotNode().WithIdentifier(name).WithLabel(name);
            Nodes.Add(name, node);
        }

        return node;
    }

    public static DotEdge GetOrCreateEdge(string from, string to)
    {
        string edgeName = EdgeName(from, to);
        Logger.LogDebug($"EDGE: {edgeName}");
        if (Edges.TryGetValue(edgeName, out DotEdge existingEdge))
        {
            Logger.LogInfo($"skipping to recreate edge '{edgeName}'");
            return existingEdge;
        }

        if (!Nodes.TryGetValue(from, out DotNode fromNode) || !Nodes.TryGetValue(to, out DotNode toNode))
            throw new Exception($"nodes for creating edge not found: from '{from}' to '{to}'");
        return GetOrCreateEdge(fromNode, toNode);
    }

    public static DotEdge GetOrCreateEdge(DotNode fromNode, DotNode toNode)
    {
        var newEdge = new DotEdge().From(fromNode).To(toNode);
        Edges.Add(EdgeName(fromNode, toNode), newEdge);
        return newEdge;
    }

    private static string EdgeName(string from, string to)
    {
        return $"{from}->-->-{to}";
    }

    private static string EdgeName(DotNode from, DotNode to)
    {
        return EdgeName(from.Identifier.Value, to.Identifier.Value);
    }
}