using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements;

[ModInitializer(nameof(Initialize))]
public class MainFile
{
    public const string ModId = "FiveElements";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        var assembly = Assembly.GetExecutingAssembly();
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
        
        //todo delete c'est 3 ligne une fois que baselib est fixé
        var targetMethod = typeof(CardModel).GetProperty(nameof(CardModel.BannerMaterial)).GetGetMethod();
        // Supprime le patch spécifique de la BaseLib pour cette méthode
        harmony.Unpatch(targetMethod, HarmonyPatchType.Prefix, "BaseLib");
        
        harmony.PatchAll();
    }
}