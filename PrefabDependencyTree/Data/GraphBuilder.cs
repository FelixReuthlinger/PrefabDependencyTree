using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using PrefabDependencyTree.Model;
using PrefabDependencyTree.Util;

namespace PrefabDependencyTree.Data;

public class GraphBuilder
{
    private readonly DotGraph Graph = new DotGraph().WithIdentifier("graph").Directed();

    private readonly Dictionary<string, DotNode> Nodes = new();
    private readonly Dictionary<string, DotEdge> Edges = new();

    public void AddNode(GraphItem item)
    {
        AddNode(item.ItemName, item.ItemType);
    }
    
    public void AddNode(string name, string nodeType)
    {
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogWarning("tried adding empty node name");
            return;
        }

        if (Nodes.ContainsKey(name)) return;
        var newNode = new DotNode()
            .WithIdentifier(name).WithLabel(name)
            .WithShape(DotNodeShape.Rectangle);
        if (nodeType != null)
        {
            if (Enum.TryParse(nodeType, out ItemDrop.ItemData.ItemType itemType))
                SetNodeColorItemType(newNode, itemType);
            else if (Enum.TryParse(nodeType, out DropType dropType))
                SetNodeColorDropType(newNode, dropType);
        }

        Nodes.Add(name, newNode);
    }

    private static void SetNodeColorDropType(DotNode node, DropType dropType)
    {
        switch (dropType)
        {
            case DropType.Character:
                node.WithColor(DotColor.Orange);
                break;
            case DropType.Container:
            case DropType.LootSpawner:
                node.WithColor(DotColor.Beige);
                break;
            case DropType.Destructible:
                node.WithColor(DotColor.Crimson);
                break;
            case DropType.Pickable:
                node.WithColor(DotColor.Ivory);
                break;
            case DropType.Tree:
                node.WithColor(DotColor.Brown);
                break;
            case DropType.MineRock:
                node.WithShape(DotNodeShape.Diamond);
                node.WithColor(DotColor.SlateGrey);
                break;
            default:
                Logger.LogWarning($"node type '{dropType.ToString()}' not supported");
                break;
        }
    }

    private static void SetNodeColorItemType(DotNode node, ItemDrop.ItemData.ItemType itemType)
    {
        switch (itemType)
        {
            case ItemDrop.ItemData.ItemType.Ammo:
            case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                node.WithColor(DotColor.Cyan);
                break;
            case ItemDrop.ItemData.ItemType.Chest:
            case ItemDrop.ItemData.ItemType.Hands:
            case ItemDrop.ItemData.ItemType.Helmet:
            case ItemDrop.ItemData.ItemType.Legs:
            case ItemDrop.ItemData.ItemType.Shoulder:
                node.WithColor(DotColor.DarkGrey);
                node.WithFontColor(DotColor.White);
                break;
            case ItemDrop.ItemData.ItemType.Tool:
            case ItemDrop.ItemData.ItemType.Torch:
            case ItemDrop.ItemData.ItemType.Utility:
                node.WithColor(DotColor.Indigo);
                break;
            case ItemDrop.ItemData.ItemType.Shield:
            case ItemDrop.ItemData.ItemType.Bow:
            case ItemDrop.ItemData.ItemType.Attach_Atgeir:
            case ItemDrop.ItemData.ItemType.OneHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                node.WithColor(DotColor.IndianRed);
                break;
            case ItemDrop.ItemData.ItemType.Consumable:
                node.WithColor(DotColor.LightGreen);
                break;
            case ItemDrop.ItemData.ItemType.Fish:
                node.WithColor(DotColor.LightBlue);
                break;
            case ItemDrop.ItemData.ItemType.Material:
                node.WithColor(DotColor.SandyBrown);
                break;
            case ItemDrop.ItemData.ItemType.Customization:
            case ItemDrop.ItemData.ItemType.Misc:
                node.WithColor(DotColor.Grey);
                node.WithFontColor(DotColor.White);
                break;
            case ItemDrop.ItemData.ItemType.None:
                node.WithColor(DotColor.Yellow);
                break;
            case ItemDrop.ItemData.ItemType.Trophy:
                node.WithColor(DotColor.Violet);
                node.WithFontColor(DotColor.White);
                break;
            default:
                throw new ArgumentException($"type '{itemType.ToString()}' not supported");
        }
    }

    public void AddEdge(string from, string to)
    {
        var edgeName = $"{from}->{to}";
        if (!Edges.ContainsKey(edgeName))
        {
            Edges.Add(edgeName, new DotEdge().From(from).To(to));
        }
    }

    public DotGraph BuildGraph()
    {
        Nodes.ToList().ForEach(node => Graph.Add(node.Value));
        Edges.ToList().ForEach(edge => Graph.Add(edge.Value));
        return Graph;
    }
}