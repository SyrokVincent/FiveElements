using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class Relic1() : StarterRelicLogic
{
    // add 1 Creation in hand at combat start
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override RelicModel? GetUpgradeReplacement() => (RelicModel) ModelDb.Relic<Relic2>();
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<Creation>(),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.ElementTuto),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ]); 
    

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        await base.BeforeHandDraw(player,choiceContext, combatState);
        // On vérifie si c'est le premier tour
        if (player.Creature.CombatState is { RoundNumber: 1 })
        {
            Flash();
            //ajout de la carte
            await FiveElementsCardExtensions.CreateInHand<Creation>(Owner, 1,false, Owner.Creature);
            await Task.CompletedTask;
        }
    }
    
    
}