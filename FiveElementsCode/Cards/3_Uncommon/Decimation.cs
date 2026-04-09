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
    
    //Reduce cost by 1 for each diferent element played this turn, deal 25
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
            .OfType<FiveElementsCard>()
            .ToList();

        int distinctElements = 0;
        if (playedCards.Any(c => c.IsWater())) distinctElements++;
        if (playedCards.Any(c => c.IsWood())) distinctElements++;
        if (playedCards.Any(c => c.IsFire())) distinctElements++;
        if (playedCards.Any(c => c.IsEarth())) distinctElements++;
        if (playedCards.Any(c => c.IsMetal())) distinctElements++;

        // On ajuste le coût de base (5) moins le nombre d'éléments distincts
        this.EnergyCost.SetThisTurn(5 - distinctElements);

    }

    /*
    private bool _waterDone = false;
    private bool _woodDone = false;
    private bool _fireDone = false;
    private bool _earthDone = false;
    private bool _metalDone = false;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {

        //Only trigger if the owner of this card play a card
        if (Owner != cardPlay.Card.Owner)
            return;
        if (cardPlay.Card is FiveElementsCard elementCard)
        {
            if ( !_waterDone && elementCard.IsWater())
            {
                this.EnergyCost.AddThisTurn(-1);
                _waterDone = true;
            }
            if ( !_woodDone && elementCard.IsWood())
            {
                this.EnergyCost.AddThisTurn(-1);
                _woodDone = true;
            }
            if ( !_fireDone && elementCard.IsFire())
            {
                this.EnergyCost.AddThisTurn(-1);
                _fireDone = true;
            }
            if ( !_earthDone && elementCard.IsEarth())
            {
                this.EnergyCost.AddThisTurn(-1);
                _earthDone = true;
            }
            if ( !_metalDone && elementCard.IsMetal())
            {
                this.EnergyCost.AddThisTurn(-1);
                _metalDone = true;
            }
        }
        await Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        _waterDone = false;
        _woodDone = false;
        _fireDone = false;
        _earthDone = false;
        _metalDone = false;
        await Task.CompletedTask;
    }*/
}