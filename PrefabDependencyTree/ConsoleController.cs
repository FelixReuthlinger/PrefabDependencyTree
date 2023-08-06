using System.Collections.Generic;
using System.IO;
using BepInEx;
using Jotunn;
using Jotunn.Entities;
using PrefabDependencyTree.Graph;

namespace PrefabDependencyTree;

public class ConsoleController : ConsoleCommand
{
    private const string prefabDependencyTreeCommand = "prefab_dependency_tree";
    private const string printOption = "print_tree";

    public override async void Run(string[] args)
    {
        Logger.LogInfo($"called '{Name}' with args '{string.Join(" ", args)}'");
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case printOption:
                    var graphBuilder = new GraphBuilder($"graph");
                    graphBuilder.ItemsToNodes(DataHarvester.Items);
                    graphBuilder.StationsToNodes(DataHarvester.CraftingStations);
                    graphBuilder.RecipesToEdges(DataHarvester.Recipes);
                    var result = await graphBuilder.Compile();
                    File.WriteAllText(
                        Path.Combine(Paths.ConfigPath, $"{PrefabDependencyTreePlugin.PluginGUID}.graph.dot"), result);
                    break;
                default:
                    Logger.LogWarning($"this option '{args[0]}' is not supported, see usage");
                    LogUsage();
                    break;
            }
        }
        else
        {
            LogUsage();
        }
    }

    private void LogUsage()
    {
        Logger.LogInfo(" - prefab_dependency_tree usage - ");
        Logger.LogInfo("command option:");
        Logger.LogInfo(
            $"   {printOption} -> will print the whole tree analyzed from game data");
    }

    public override string Name => prefabDependencyTreeCommand;
    public override string Help => "Prefab Dependency Tree Console Commands";

    public override List<string> CommandOptionList() => new() { printOption };
}