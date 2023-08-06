using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetGraph.Attributes;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Jotunn;
using PrefabDependencyTree.Model;

namespace PrefabDependencyTree.Graph;

public class GraphBuilder
{
    private readonly DotGraph graph;
    private readonly Dictionary<string, DotNode> nodes = new();
    private readonly Dictionary<string, DotEdge> edges = new();

    public GraphBuilder(string name)
    {
        graph = new DotGraph().WithIdentifier(name).Directed();
    }

    public void ItemsToNodes(Dictionary<string, SimpleItem> items)
    {
        foreach (KeyValuePair<string, SimpleItem> item in items)
        {
            var newNode = new DotNode()
                .WithIdentifier(item.Key)
                .WithLabel(item.Key)
                .WithShape(DotNodeShape.Box);
            nodes.Add(item.Key, newNode);
        }
    }

    public void StationsToNodes(Dictionary<string, SimpleCraftingStation> stations)
    {
        foreach (KeyValuePair<string, SimpleCraftingStation> station in stations)
        {
            var newNode = new DotNode()
                .WithIdentifier(station.Key)
                .WithLabel(station.Key)
                .WithShape(DotNodeShape.Diamond);
            nodes.Add(station.Key, newNode);
        }
    }

    private DotNode RecipeNode(SimpleRecipe recipe)
    {
        string nodeName = recipe.RecipeName();
        var recipeNode = new DotNode()
            .WithIdentifier(nodeName)
            .WithShape(DotNodeShape.Note);
        nodes.Add(nodeName, recipeNode);
        return recipeNode;
    }

    private void AttachStationToRecipe(SimpleRecipe recipe, DotNode recipeNode)
    {
        string stationName = recipe.Station?.stationName;
        if (stationName == null) return;
        if (nodes.TryGetValue(stationName, out DotNode stationNode))
            FromToEdge(stationNode, recipeNode, optionalLabel: $"min lvl {recipe.Station.minStationLevel}");
        else
            Logger.LogWarning($"could not find station '{stationName}'");
    }

    private void IncreaseUsageCounter(DotNode node)
    {
        int used = 1;
        if (node.TryGetAttribute("used", out DotAttribute alreadyUsed)) used += int.Parse(alreadyUsed.Value);
        node.SetAttribute("used", new DotAttribute(used.ToString()));
    }

    private void FromToEdge(DotNode from, DotNode to, string optionalLabel = "")
    {
        string edgeName = $"{from.Identifier.Value} to {to.Identifier.Value}";
        var edge = new DotEdge().From(from).To(to).WithLabel(optionalLabel);
        IncreaseUsageCounter(from);
        IncreaseUsageCounter(to);
        edges.Add(edgeName, edge);
    }

    public void RecipesToEdges(Dictionary<string, SimpleRecipe> recipes)
    {
        foreach (KeyValuePair<string, SimpleRecipe> recipe in recipes)
        {
            if (nodes.TryGetValue(recipe.Value.Item.ItemName, out DotNode craftedItemNode))
            {
                var recipeNode = RecipeNode(recipe.Value);
                AttachStationToRecipe(recipe.Value, recipeNode);
                FromToEdge(recipeNode, craftedItemNode);

                foreach (IngredientsRequirement ingredient in recipe.Value.Ingredients)
                {
                    if (nodes.TryGetValue(ingredient.Item.ItemName, out DotNode ingredientNode))
                        FromToEdge(ingredientNode, recipeNode, optionalLabel: $"x {ingredient.ItemAmount}");
                    else
                        Logger.LogWarning($"could not find ingredient '{ingredient.Item.ItemName}' for recipe to " +
                                          $"create item '{recipe.Value.Item.ItemName}");
                }
            }
            else Logger.LogWarning($"could not find crafting result '{recipe.Value.Item.ItemName}' item");
        }
    }

    public async Task<string> Compile()
    {
        foreach (KeyValuePair<string, DotNode> node in nodes)
        {
            if (node.Value.TryGetAttribute("used", out DotAttribute used) && int.Parse(used.Value) > 0)
                graph.Add(node.Value);
        }

        foreach (KeyValuePair<string,DotEdge> edge in edges)
        {
            graph.Add(edge.Value);
        }

        var writer = new StringWriter();
        await graph.CompileAsync(new CompilationContext(writer, new CompilationOptions()));
        return writer.GetStringBuilder().ToString();
    }
}