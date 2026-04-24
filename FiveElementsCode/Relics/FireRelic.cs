using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class FireRelic() : FiveElementsRelic
{
    //At start of combat add 2 fire plume in hand
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(2),
    ]);

 
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<FirePlume>(),
    ]); 
    

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        await base.BeforeHandDraw(player,choiceContext, combatState);
        // On vérifie si c'est le premier tour
        if (player.Creature.CombatState is { RoundNumber: 1 })
        {
            Flash();
            //ajout des carte
            await FiveElementsCardExtensions.CreateInHand<FirePlume>(Owner, DynamicVars.Cards.IntValue,false, combatState);
            await Task.CompletedTask;
        }
    }

    
}
