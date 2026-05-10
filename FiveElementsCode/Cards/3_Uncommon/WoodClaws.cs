using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WoodClaws() : WoodCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(Owner.Creature);

    //Deal 7 damage,
    //Wood:(replay for each other wood card played this turn) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(7,ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ReplayCount").WithMultiplier((card, target) =>
            ElementHistoryUtils.CountPlayedCardsOfElement(card.CombatState, card.Owner, CardElementTag.Wood))
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ]);
    

    // Une simple variable suffit car l'AutoPlay réutilise cette instance d'objet
    private decimal _remainingReplays = -1;
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
    
        if (CardElementTag.Wood.IsActive(Owner.Creature))
        {
            // 1. Initialisation si c'est le début d'une chaîne (peu importe qui l'a lancée)
            if (_remainingReplays == -1) 
            {
                _remainingReplays = DynamicVars["ReplayCount"].PreviewValue;
            }

            // 2. Si on a des répétitions à faire, on décrémente et on relance
            if (_remainingReplays > 0)
            {
                _remainingReplays--;
                await CardCmd.AutoPlay(context, this, cardPlay.Target);
            }
        
            // 3. RESET CRITIQUE : 
            // Si on est à 0 (fin de chaîne ou initialisé à 0), on repasse à -1 
            // pour que la carte soit prête pour le prochain tour ou le prochain OnPlay.
            if (_remainingReplays == 0) 
            {
                _remainingReplays = -1;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}