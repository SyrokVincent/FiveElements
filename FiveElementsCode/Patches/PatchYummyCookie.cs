using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Relics;
using FiveElements.FiveElementsCode.Character;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;

[HarmonyPatch(typeof(NRelic), "Reload")] 
public static class PatchYummyCookieVisual
{
    private static Texture2D _myCookieIcon;
    private static Texture2D _myCookieOutline;
    private static Texture2D _myCookieBig;

    public static void Postfix(NRelic __instance)
    {
        
        // Vérifie si c'est le biscuit
        if (__instance.Model is YummyCookie)
        {
            // Condition : Soit on a un Owner qui est ton perso, 
            // soit on est en "Canonical" (menu/bibliothèque) et on veut forcer l'icône
            bool isMyCharacter = __instance.Model.Owner?.Character is FiveElements.FiveElementsCode.Character.FiveElements;
            bool isLibrary = __instance.Model.IsCanonical;

            if (isMyCharacter || isLibrary)
            {
                // 2. Chargement des textures depuis ton projet
                if (_myCookieIcon == null)
                {
                    // Note : Ajuste bien les noms de fichiers selon tes besoins
                    _myCookieIcon = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage.png");
                    _myCookieOutline =
                        GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage_outline.png");
                    _myCookieBig = GD.Load<Texture2D>("res://FiveElements/images/relics/big/yummy_cookie_sage.png");
                }

                // 3. Récupération des IconSize via réflexion (car le champ _iconSize est privé)
                var iconSizeField = typeof(NRelic).GetField("_iconSize",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var currentSize = (NRelic.IconSize)iconSizeField.GetValue(__instance);

                // 4. Application des textures selon la taille demandée par STS2
                if (currentSize == NRelic.IconSize.Small)
                {
                    __instance.Icon.Texture = _myCookieIcon;
                    __instance.Outline.Visible = true;
                    __instance.Outline.Texture = _myCookieOutline;
                }
                else if (currentSize == NRelic.IconSize.Large)
                {
                    __instance.Icon.Texture = _myCookieBig;
                    __instance.Outline.Visible = false;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(NInspectRelicScreen), "UpdateRelicDisplay")]
public static class PatchInspectRelicCookie
{
    private static Texture2D _myCookieBig;

    public static void Postfix(NInspectRelicScreen __instance)
    {
        // 1. Accéder aux champs privés de l'écran via réflexion
        var indexField = typeof(NInspectRelicScreen).GetField("_index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var relicsField = typeof(NInspectRelicScreen).GetField("_relics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var imageField = typeof(NInspectRelicScreen).GetField("_relicImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (indexField == null || relicsField == null || imageField == null) return;

        // 2. Récupérer la relique actuellement affichée
        int index = (int)indexField.GetValue(__instance);
        var relics = (IReadOnlyList<RelicModel>)relicsField.GetValue(__instance);
        var relicModel = relics[index];
        var relicImage = (TextureRect)imageField.GetValue(__instance);

        // 3. Appliquer la logique de remplacement
        // On vérifie si c'est le biscuit ET si le perso est le Sage (ou si on est dans la bibliothèque)
        if (relicModel is YummyCookie && (relicModel.Owner?.Character is FiveElements.FiveElementsCode.Character.FiveElements || relicModel.IsCanonical))
        {
            // On ne change l'image que si elle est censée être visible (pas verrouillée)
            if (relicImage.SelfModulate != StsColors.ninetyPercentBlack && relicImage.Visible)
            {
                if (_myCookieBig == null)
                {
                    _myCookieBig = GD.Load<Texture2D>("res://FiveElements/images/relics/big/yummy_cookie_sage.png");
                }

                if (_myCookieBig != null)
                {
                    relicImage.Texture = _myCookieBig;
                }
            }
        }
    }
}