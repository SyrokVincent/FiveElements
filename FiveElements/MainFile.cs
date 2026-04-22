using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace FiveElements;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "FiveElements"; //Used for resource filepath

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        var assembly = Assembly.GetExecutingAssembly();
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
        harmony.PatchAll();
    }
}