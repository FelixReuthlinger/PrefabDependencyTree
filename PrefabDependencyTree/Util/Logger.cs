using BepInEx.Logging;

namespace PrefabDependencyTree.Util;

public static class Logger
{
    private static readonly ManualLogSource LoggerInstance =
        BepInEx.Logging.Logger.CreateLogSource(PrefabDependencyTreePlugin.PluginName);

    public static void LogDebug(string text) => LoggerInstance.LogDebug(text);
    
    public static void LogInfo(string text) => LoggerInstance.LogInfo(text);

    public static void LogWarning(string text) => LoggerInstance.LogWarning(text);

    public static void LogError(string text) => LoggerInstance.LogError(text);
}