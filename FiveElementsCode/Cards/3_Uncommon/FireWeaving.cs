using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class FireWeaving() : FireCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Ethereal, Exhaust 1 non-fire card at random. Deal 6 Heat damage, Fire:(return in hand)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(6,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Ethereal,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // 1. Trouver une carte non-feu au hasard dans la main (autre que celle-ci)
        var nonFireCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c != this && !c.CountsAsElement(CardElementTag.Fire, Owner.Creature))
            .ToList();
        
        var randomTargetToExhaust = Owner.RunState.Rng.CombatCardSelection.NextItem(nonFireCards);
        if (randomTargetToExhaust != null)
        {
            await CardCmd.Exhaust(choiceContext, randomTargetToExhaust);
        }
        
        await DealHeatDamage(choiceContext, play.Target, DynamicVars.Damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to hand if fire is active
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return (resultPileType is PileType.Discard or PileType.Exhaust &&
                CardElementTag.Fire.IsActive(CombatState)) ? PileType.Hand : resultPileType;
    }
    
}






// old card stuff
/*
public sealed class FireWeaving() : FireCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Exhaust a non-fire card or itself if you can't. Deal 6 Heat damage, Fire:(return in hand if you exhausted a card)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(6,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        // select of a non-fire card
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => !(c is FiveElementsCard f && f.IsFire()) && c != this,
            this
        );
        
        
        var selectedCard = selection?.FirstOrDefault();
        
        if (selectedCard != null)
        {
            // another card is exhausted
            await CardCmd.Exhaust(choiceContext, selectedCard);
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.Damage);
            
        }
        else
        {
            //the card exhaust itself (returning to hand is done by getresultpile
            await DealHeatDamage(choiceContext, play.Target, DynamicVars.Damage);
            await CardCmd.Exhaust(choiceContext, this);
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to hand if fire is active (and also if the card don't exhaust itself)
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return (resultPileType is PileType.Discard or PileType.Exhaust &&
                CardElementTag.Fire.IsActive(CombatState)) ? PileType.Hand : resultPileType;
    }
    
}
*/