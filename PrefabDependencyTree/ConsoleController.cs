﻿using System.Collections.Generic;
using System.IO;
using BepInEx;
using DotNetGraph.Compilation;
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
                    var writer = new StringWriter();
                    var context = new CompilationContext(writer, new CompilationOptions());
                    var graph = GraphBuilder.CreateGraph();
                    await graph.CompileAsync(context);
                    string outputFilePath = Path.Combine(Paths.ConfigPath,
                        $"{PrefabDependencyTreePlugin.PluginGUID}.graph.dot");
                    File.WriteAllText(outputFilePath, writer.GetStringBuilder().ToString());
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