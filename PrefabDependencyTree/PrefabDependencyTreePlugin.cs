using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace PrefabDependencyTree
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class PrefabDependencyTreePlugin : BaseUnityPlugin
    {
        public const string PluginAuthor = "FixItFelix";
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginName = "PrefabDependencyTree";
        public const string PluginVersion = "1.0.0";
        

        private void Awake()
        {
        
        }
    }
}

