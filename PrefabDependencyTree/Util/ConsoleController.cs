using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using DotNetGraph.Compilation;
using Jotunn.Entities;
using PrefabDependencyTree.Data;

namespace PrefabDependencyTree.Util;

public class ConsoleController : ConsoleCommand
{
    private const string prefabDependencyTreeCommand = "prefab_dependency_tree";
    private const string printOption = "print_tree";
    private const string printIncludeFilteredOption = "print_include_filtered_tree";
    private const string printExcludeFilteredOption = "print_exclude_filtered_tree";
    private const string debugLogAll = "debug_log_all";

    private static readonly List<string> ItemTypeEnums = Enum.GetNames(typeof(ItemDrop.ItemData.ItemType)).ToList();

    private static readonly List<ItemDrop.ItemData.ItemType> WeaponsAndArmorEnums = new()
    {
        ItemDrop.ItemData.ItemType.Ammo,
        ItemDrop.ItemData.ItemType.Bow,
        ItemDrop.ItemData.ItemType.Chest,
        ItemDrop.ItemData.ItemType.Customization,
        ItemDrop.ItemData.ItemType.Hands,
        ItemDrop.ItemData.ItemType.Helmet,
        ItemDrop.ItemData.ItemType.Legs,
        ItemDrop.ItemData.ItemType.Shield,
        ItemDrop.ItemData.ItemType.Shoulder,
        ItemDrop.ItemData.ItemType.Tool,
        ItemDrop.ItemData.ItemType.Torch,
        ItemDrop.ItemData.ItemType.Utility,
        ItemDrop.ItemData.ItemType.Attach_Atgeir,
        ItemDrop.ItemData.ItemType.AmmoNonEquipable,
        ItemDrop.ItemData.ItemType.OneHandedWeapon,
        ItemDrop.ItemData.ItemType.TwoHandedWeapon,
        ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
    };

    private static readonly List<string> WeaponsAndArmorTypes =
        WeaponsAndArmorEnums.Select(enumEntry => enumEntry.ToString()).ToList();

    private const string WeaponsAndArmorTypesName = "WeaponsAndArmorTypes";

    private static List<string> ReplaceTypes(List<string> input)
    {
        if (!input.Contains(WeaponsAndArmorTypesName)) return input;
        input.Remove(WeaponsAndArmorTypesName);
        input.AddRange(WeaponsAndArmorTypes);
        return input;
    }

    public override void Run(string[] args)
    {
        Logger.LogInfo($"called '{Name}' with args '{string.Join(" ", args)}'");
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case printOption:
                    WriteGraphOutput(filteredData: new IncludeFilterTypes(itemTypeFilters: ItemTypeEnums));
                    break;
                case printIncludeFilteredOption:
                    if (args.Length > 1)
                        WriteGraphOutput(
                            filteredData: new IncludeFilterTypes(
                                itemTypeFilters: ReplaceTypes(args[1].Split(',').ToList())
                            )
                        );
                    else
                    {
                        Logger.LogWarning("you did not provide item types to filter for, see usage");
                        LogUsage();
                    }

                    break;
                case printExcludeFilteredOption:
                    if (args.Length > 1)
                        WriteGraphOutput(
                            filteredData: new ExcludeFilterTypes(
                                itemTypeFilters: ReplaceTypes(args[1].Split(',').ToList())
                            )
                        );
                    else
                    {
                        Logger.LogWarning("you did not provide item types to filter for, see usage");
                        LogUsage();
                    }

                    break;
                case debugLogAll:
                    List<string> resultString = DataHarvester.LogAllToString();
                    resultString.ForEach(Logger.LogWarning);
                    string filePath = Path.Combine(Paths.ConfigPath,
                        $"{PrefabDependencyTreePlugin.PluginGUID}.all.txt");
                    File.WriteAllText(path: filePath, contents: string.Join("\n", resultString));
                    Logger.LogInfo($"wrote file '{filePath}'");
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

    private static async void WriteGraphOutput(FilteredData filteredData)
    {
        using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        filteredData.LogOverview();
        var graph = filteredData.CreateGraph();
        await graph.CompileAsync(context);
        string outputFilePath = Path.Combine(Paths.ConfigPath, $"{PrefabDependencyTreePlugin.PluginGUID}.graph.dot");
        File.WriteAllText(outputFilePath, writer.GetStringBuilder().ToString());
        Logger.LogInfo($"wrote graph file '{outputFilePath}'");
    }

    private static void LogUsage()
    {
        Logger.LogInfo(" - prefab_dependency_tree usage - ");
        Logger.LogInfo("command option:");
        Logger.LogInfo($"   {printOption} -> will print the whole tree analyzed from game data");
        Logger.LogInfo($"   {debugLogAll} -> will log warn all prefabs (this will be huge log output!)");
        Logger.LogInfo($"   {printIncludeFilteredOption} Material,Consumable -> " +
                       $"print tree with items of the provided item types included " +
                       $"(complete tree with link to any included items)");
        Logger.LogInfo($"   {printExcludeFilteredOption} Ammo,OneHandedWeapon -> " +
                       $"print tree with items of the provided item types excluded " +
                       $"(remove the provided types from tree)");
        Logger.LogInfo($"        all item types: {string.Join(", ", ItemTypeEnums)}, {WeaponsAndArmorTypesName}");
    }

    public override string Name => prefabDependencyTreeCommand;
    public override string Help => "Prefab Dependency Tree Console Commands";

    public override List<string> CommandOptionList() =>
        new() { printOption, printIncludeFilteredOption, printExcludeFilteredOption, debugLogAll };
}