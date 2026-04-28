using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class More() : NeutralCard(1,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    // need a few card with strike tag
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    //Deal 5 damage, deal 1 more for each card in your deck of the element you have the more of  (for big mono element deck)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(5), // Dégâts de base
        new ExtraDamageVar(1),    // Dégâts bonus par carte
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
           // Récupérer toutes les cartes présentes dans le combat
           var allCards = card.Owner.PlayerCombatState?.AllCards;

           // Compter les occurrences de chaque élément (en ignorant le Neutre)
           if (allCards != null)
           {
               var elementCounts = allCards
                   .OfType<FiveElementsCard>()
                   .SelectMany(c => c.ElementTags)
                   .Where(t => t != CardElementTag.Neutral)
                   .GroupBy(t => t)
                   .Select(group => group.Count())
                   .ToList();

               // Trouver le maximum (0 si aucune carte élémentaire n'est trouvée)
               int maxCount = elementCounts.Any() ? elementCounts.Max() : 0;
            
               // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
               return maxCount;
           }

           return 0;
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Attune,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo)
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
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
}