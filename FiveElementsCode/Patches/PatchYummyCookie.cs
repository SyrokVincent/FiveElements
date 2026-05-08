using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Relics;
using FiveElements.FiveElementsCode.Character;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;
/*
[HarmonyPatch(typeof(NRelic), "Reload")]
public static class PatchYummyCookieVisual
{
    private static Texture2D _myCookieIcon;
    private static Texture2D _myCookieOutline;
    private static Texture2D _myCookieBig;

    public static void Postfix(NRelic __instance)
    {
        // --- FIX ANTI-CRASH ---
        // On accède au champ privé _model pour vérifier s'il est null sans déclencher l'exception
        var modelField = typeof(NRelic).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var modelValue = modelField?.GetValue(__instance) as RelicModel;

        if (modelValue == null) return; // Si pas de modèle, on ne fait rien, Reload sera rappelé plus tard
        // -----------------------

        if (modelValue is YummyCookie && modelValue.Owner?.Character is FiveElements.FiveElementsCode.Character.FiveElements)
        {
            if (_myCookieIcon == null)
            {
                _myCookieIcon = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage.png");
                _myCookieOutline = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage_outline.png");
                _myCookieBig = GD.Load<Texture2D>("res://FiveElements/images/relics/big/yummy_cookie_sage.png");
            }

            var iconSizeField = typeof(NRelic).GetField("_iconSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentSize = (NRelic.IconSize)iconSizeField.GetValue(__instance);

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
*/

[HarmonyPatch(typeof(NRelic), "Reload")]
public static class PatchYummyCookieVisual
{
    private static Texture2D _myCookieIcon;
    private static Texture2D _myCookieOutline;
    private static Texture2D _myCookieBig;

    public static void Postfix(NRelic __instance)
    {
        // 1. Accès sécurisé au modèle (déjà bien fait avec ta réflexion)
        var modelField = typeof(NRelic).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var modelValue = modelField?.GetValue(__instance) as RelicModel;

        if (modelValue == null) return;

        // --- LA FIX EST ICI ---
        // On vérifie si c'est la bonne relique
        if (modelValue is YummyCookie)
        {
            // IMPORTANT: Si la relique est "Canonical" (dans la collection/bibliothèque), 
            // elle n'a PAS de Owner. On doit quand même changer le visuel.
            bool isOurCharacter = modelValue.IsCanonical || 
                                 (modelValue.Owner != null && modelValue.Owner.Character is FiveElements.FiveElementsCode.Character.FiveElements);

            if (isOurCharacter)
            {
                ApplyTextures(__instance);
            }
        }
    }

    private static void ApplyTextures(NRelic __instance)
    {
        if (_myCookieIcon == null)
        {
            _myCookieIcon = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage.png");
            _myCookieOutline = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage_outline.png");
            _myCookieBig = GD.Load<Texture2D>("res://FiveElements/images/relics/big/yummy_cookie_sage.png");
        }

        var iconSizeField = typeof(NRelic).GetField("_iconSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentSize = (NRelic.IconSize)iconSizeField.GetValue(__instance);

        if (currentSize == NRelic.IconSize.Small)
        {
            if (__instance.Icon != null) __instance.Icon.Texture = _myCookieIcon;
            if (__instance.Outline != null)
            {
                __instance.Outline.Visible = true;
                __instance.Outline.Texture = _myCookieOutline;
            }
        }
        else if (currentSize == NRelic.IconSize.Large)
        {
            if (__instance.Icon != null) __instance.Icon.Texture = _myCookieBig;
            if (__instance.Outline != null) __instance.Outline.Visible = false;
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
[HarmonyPatch(typeof(NEventOptionButton), "_Ready")]
public static class PatchAncientEventCookieIcon
{
    private static Texture2D? _myCookieIcon;
    private static Texture2D? _myCookieOutline;

    public static void Postfix(NEventOptionButton __instance)
    {
        // 1. Vérifier si l'option contient notre relique
        if (__instance.Option?.Relic is YummyCookie)
        {
            // 2. not Optionnel : Vérifier si c'est le personnage "FiveElements" 
            if ( __instance.Option.Relic.Owner?.Character is FiveElements.FiveElementsCode.Character.FiveElements)
            {
                // 3. Charger les textures personnalisées
                if (_myCookieIcon == null)
                {
                    _myCookieIcon = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage.png");
                    _myCookieOutline = GD.Load<Texture2D>("res://FiveElements/images/relics/yummy_cookie_sage_outline.png");
                }

                // 4. Accéder aux nodes via les Unique Names (%) définis dans ton décompilage
                var relicIcon = __instance.GetNodeOrNull<TextureRect>("%RelicIcon");
                var relicOutline = __instance.GetNodeOrNull<TextureRect>("%Outline");

                if (relicIcon != null && _myCookieIcon != null)
                {
                    relicIcon.Texture = _myCookieIcon;
                }

                if (relicOutline != null && _myCookieOutline != null)
                {
                    relicOutline.Texture = _myCookieOutline;
                }
            }
        }
    }
}