using BaseLib.Utils;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class EarthFulu() : EarthCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    
    //Exhaust, Shift (Ethereal?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(2,ValueProp.Move),
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
        // 1. On ne se transforme que si une AUTRE carte est jouée par le propriétaire
        if (cardPlay.Card == this || Owner != cardPlay.Card.Owner) return;
        
        await this.TryShiftFuluTransform();
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        //nothing to do ?
        await CommonActions.CardBlock(this, play);
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Ethereal);
        this.AddKeyword(CardKeyword.Retain);
    }
}