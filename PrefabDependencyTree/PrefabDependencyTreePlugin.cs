using BepInEx;
using Jotunn;
using Jotunn.Managers;
using PrefabDependencyTree.Data;

namespace PrefabDependencyTree
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Main.ModGuid)]
    internal class PrefabDependencyTreePlugin : BaseUnityPlugin
    {
        public const string PluginAuthor = "FixItFelix";
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginName = "PrefabDependencyTree";
        public const string PluginVersion = "1.0.0";

        private void Awake()
        {
            PrefabManager.OnPrefabsRegistered += DataHarvester.Initialize;
            CommandManager.Instance.AddConsoleCommand(new ConsoleController());
        }
    }
}

