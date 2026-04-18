using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public class Less() : NeutralCard(1,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    // need a few card with strike tag
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    //Deal 5 damage, deal 2 more for each card in your deck of the element you have the least of  (for big multi element deck)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(5), // Dégâts de base
        new ExtraDamageVar(5),    // Dégâts bonus par carte
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            var allCards = card.Owner.PlayerCombatState?.AllCards;

            // On prépare le compte pour les 5 éléments
            var counts = new Dictionary<CardElementTag, int>
            {
                { CardElementTag.Fire, 0 },
                { CardElementTag.Water, 0 },
                { CardElementTag.Wood, 0 },
                { CardElementTag.Earth, 0 },
                { CardElementTag.Metal, 0 }
            };

            // On compte les elements des cartes présentes dans le deck
            if (allCards != null)
                foreach (var c in allCards)
                {
                    if (c is FiveElementsCard fec)
                    {
                        foreach (var tag in fec.ElementTags)
                        {
                            if (counts.ContainsKey(tag))
                            {
                                counts[tag]++;
                            }
                        }
                    }
                }

            // On prend le minimum absolu parmi les 5
            var minCount = counts.Values.Min();
    
            return minCount;
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Shift,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        await base.OnPlay(choiceContext, play);
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {

        DynamicVars.CalculationBase.UpgradeValueBy(3);
        DynamicVars.ExtraDamage.UpgradeValueBy(3);
    }
}