using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Patches;



[HarmonyPatch(typeof(HoverTipFactory), "FromKeyword")] 
[HarmonyPriority(Priority.High)] // Force mon patch à s'exécuter en premier, todo a voir si ça cause pas d'autre probleme
public class FromKeywordPatch
{   
    //color for element in cards description
    private const string WaterColor = "[color=#1E90FF]";
    private const string WoodColor = "[color=#228B22]";
    private const string FireColor = "[color=#FF4500]";
    private const string EarthColor = "[color=#8B4513]";
    private const string MetalColor = "[color=#C0C0C0]";
    
    public static CardElementTag ElementOfEcho => Character.FiveElements.Echo;
    
    protected static IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("water_s",WaterColor),
        new StringVar("water_e","[/color]"),
        new StringVar("wood_s",WoodColor),
        new StringVar("wood_e","[/color]"),
        new StringVar("fire_s",FireColor),
        new StringVar("fire_e", "[/color]"),
        new StringVar("earth_s", EarthColor),
        new StringVar("earth_e", "[/color]"),
        new StringVar("metal_s", MetalColor ),
        new StringVar("metal_e", "[/color]" ),
    ];
    
    [HarmonyPrefix] // Optionnel si tu respectes le nom "Prefix", mais plus explicite
    static bool Prefix(CardKeyword keyword, ref IHoverTip __result)
    {
        if (keyword == FiveElementsKeywords.Echo ||
            keyword == FiveElementsKeywords.Generate || 
            keyword == FiveElementsKeywords.Shift || 
            keyword == FiveElementsKeywords.Water || 
            keyword == FiveElementsKeywords.Wood ||
            keyword == FiveElementsKeywords.Fire ||
            keyword == FiveElementsKeywords.Earth ||
            keyword == FiveElementsKeywords.Metal||
            keyword == FiveElementsKeywords.Fireboost
            )
        {
            //cree locstring with dynamic var I want, then put inside hovertip
            var title = keyword.GetTitle();
            var description = keyword.GetDescription();
            foreach (DynamicVar var in CanonicalVars)
            {
                title.Add(var);
                description.Add(var);
            }
            
            if (keyword == FiveElementsKeywords.Echo)
            {
                var elemEcho = new IntVar("ElemEcho", (decimal)ElementOfEcho);
                title.Add(elemEcho);
                description.Add(elemEcho);
            }
            
            __result = new HoverTip(title, description);
            
            return false; // Tell harmony to NOT execute originale method
        }
        
        return true; // For other keyword we let the original method do it's things
    }
    
    
    
    
    
    
}
/*
 if (!HoverTipFactory._keywordHoverTips.ContainsKey(keyword))
      HoverTipFactory._keywordHoverTips[keyword] = new HoverTip(keyword.GetTitle(), keyword.GetDescription());
    return (IHoverTip) HoverTipFactory._keywordHoverTips[keyword];
 */