using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class Decimation() : NeutralCard(5,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    //Reduce cost by 1 for each diferent element played this turn, deal 20
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(20, ValueProp.Move)
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }


    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var playedCards = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner)
            .Select(e => e.CardPlay.Card)
            //.OfType<FiveElementsCard>()
            .ToList();

        int distinctElements = 0;
        if (playedCards.Any(c => c.CountsAsElement(CardElementTag.Water,cardPlay.Card.Owner.Creature))) distinctElements++;
        if (playedCards.Any(c => c.CountsAsElement(CardElementTag.Wood,cardPlay.Card.Owner.Creature))) distinctElements++;
        if (playedCards.Any(c => c.CountsAsElement(CardElementTag.Fire,cardPlay.Card.Owner.Creature))) distinctElements++;
        if (playedCards.Any(c => c.CountsAsElement(CardElementTag.Earth,cardPlay.Card.Owner.Creature))) distinctElements++;
        if (playedCards.Any(c => c.CountsAsElement(CardElementTag.Metal,cardPlay.Card.Owner.Creature))) distinctElements++;

        // On ajuste le coût de base (5) moins le nombre d'éléments distincts
        this.EnergyCost.SetThisTurn(5 - distinctElements);

    }
    
}