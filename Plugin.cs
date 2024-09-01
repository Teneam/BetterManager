using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterManager.Patches;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using static HarmonyLib.AccessTools;

namespace BetterManager;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static Harmony Harmony;

    private ConfigEntry<bool> ConfigPricingEnhancement;
    private ConfigEntry<bool> ConfigTotalDisplay;

    //public static bool EmployeeCollisions => Instance.ConfigPricingEnhancement.Value;
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        // Set up config options
        ConfigPricingEnhancement = Config.Bind("Stock", "Pricing Ordering", true, "Allow ordering using 'E' from pricing gun.");
        ConfigTotalDisplay = Config.Bind("Stock", "Total Display", true, "Display Total available shelf space for products.");

        Harmony = new("bettermanager");

        // Enable features based on config
        if (ConfigPricingEnhancement.Value)
            Harmony.PatchAll(typeof(PricingOrder));

        if (ConfigTotalDisplay.Value)
            Harmony.PatchAll(typeof(DisplayTotal));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");


    }

}