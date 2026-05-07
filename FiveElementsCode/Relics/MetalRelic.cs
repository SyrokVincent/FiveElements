using BaseLib.Utils;
using FiveElements.FiveElementsCode.Character;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class MetalRelic() : FiveElementsRelic
{
    //At start of each turn gain 4 vigor
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(4), 
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>()
    ]); 
    
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        
        this.Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext,Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature,null);
    }
    
}
