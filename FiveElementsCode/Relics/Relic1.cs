using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Cards._6_Ancient;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public class Relic1() : StarterRelicLogic
{

    public override RelicModel? GetUpgradeReplacement() => (RelicModel) ModelDb.Relic<Relic2>();
    
    public override RelicRarity Rarity => RelicRarity.Starter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<Creation>(),
    ]); 
    

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        await base.BeforeHandDraw(player,choiceContext, combatState);
        
        // On vérifie si c'est le premier tour
        if (player == Owner && player.Creature.CombatState is { RoundNumber: 1 })
        {
            //ajout de la carte
            await FiveElementsCardExtensions.CreateInHand<Creation>(Owner, 1,false, combatState);
            await Task.CompletedTask;
        }
    }
    
    
}