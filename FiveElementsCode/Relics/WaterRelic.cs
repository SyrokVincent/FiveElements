using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class WaterRelic() : FiveElementsRelic
{
    //now wave trigger at turn end
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]); 
   
    
}
