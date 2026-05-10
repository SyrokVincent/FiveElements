using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class WaterRelic() : FiveElementsRelic
{
    //now wave trigger at turn end
    //added 5 wave at combat start
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WavePower>(5),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]); 
    
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        await base.BeforeHandDraw(player,choiceContext, combatState);
        // On vérifie si c'est le premier tour
        if (player.Creature.CombatState is { RoundNumber: 1 })
        {
            Flash();
            await PowerCmd.Apply<WavePower>(choiceContext, Owner.Creature, this.DynamicVars["WavePower"].BaseValue, this.Owner.Creature, null);
        }
    }
    
}
