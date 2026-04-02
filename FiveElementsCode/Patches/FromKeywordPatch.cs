using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Patches;



[HarmonyPatch(typeof(HoverTipFactory), "FromKeyword")] 
public class FromKeywordPatch
{   
    //color for element in cards description
    private const string WaterColor = "[color=#1E90FF]";
    private const string WoodColor = "[color=#228B22]";
    private const string FireColor = "[color=#FF4500]";
    private const string EarthColor = "[color=#8B4513]";
    private const string MetalColor = "[color=#C0C0C0]";
    
    public static CardElementTag ElementOfEcho => Relic1.Echo;
    
    protected static IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("water_s",CardElementTag.Water.IsActive() ? WaterColor : "" ),
        new StringVar("water_e",CardElementTag.Water.IsActive() ? "[/color]" : ""),
        new StringVar("wood_s",CardElementTag.Wood.IsActive() ? WoodColor : ""),
        new StringVar("wood_e",CardElementTag.Wood.IsActive() ? "[/color]" : ""),
        new StringVar("fire_s",CardElementTag.Fire.IsActive() ? FireColor : ""),
        new StringVar("fire_e",CardElementTag.Fire.IsActive() ? "[/color]" : ""),
        new StringVar("earth_s",CardElementTag.Earth.IsActive() ? EarthColor : ""),
        new StringVar("earth_e",CardElementTag.Earth.IsActive() ? "[/color]" : ""),
        new StringVar("metal_s",CardElementTag.Metal.IsActive() ? MetalColor : ""),
        new StringVar("metal_e",CardElementTag.Metal.IsActive() ? "[/color]" : ""),
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
            keyword == FiveElementsKeywords.Metal
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