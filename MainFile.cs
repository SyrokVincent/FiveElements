using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

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
        harmony.PatchAll();
    }
}