using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace FiveElements.FiveElementsCode.Cards.Token;

[Pool(typeof(TokenCardPool))]
public sealed class ElementalFulu() : NeutralCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self, true, true)
{
    //Exhaust, Shift (Ethereal?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal,
        FiveElementsKeywords.Shift,
        FiveElementsKeywords.Echo,
        FiveElementsKeywords.Generate,
        CardKeyword.Exhaust, 
    ];


    //change element when a card is played
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await this.TryShiftFuluTransform(cardPlay);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        //nothing to do ?
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Ethereal);
        this.AddKeyword(CardKeyword.Retain);
    }

    public static async Task CreateInHand(Player owner, int count, bool isUpgraded, CombatState combatState)
    {var fulus = new List<CardModel>();
    
        for (var i = 0; i < count; i++) 
        {
            var fulu = combatState.CreateCard<ElementalFulu>(owner);
            
            if (isUpgraded)
            {
                CardCmd.Upgrade(fulu);
            }
            fulus.Add(fulu);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(fulus, PileType.Hand, true);
    }
}