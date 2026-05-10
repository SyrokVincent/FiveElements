using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WoodSeed() : WoodCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(Owner.Creature);

    //If this is the first time this card has been played this turn, draw 1 card.
    //Wood:(Gain 2(3) Surge)
    //swapped the thing and added limit per turn
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<SurgePower>(2),
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<SurgePower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        //await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        if (!HasBeenPlayedThisTurn)
        {
            await CommonActions.Draw(this, choiceContext);
        }
        
        if (CardElementTag.Wood.IsActive(Owner.Creature))
        {
            await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["SurgePower"].UpgradeValueBy(1);
    }
    
    // LOGIQUE DE VÉRIFICATION DU TOUR
    private bool HasBeenPlayedThisTurn
    {
        get
        {
            // On fouille dans l'historique des cartes terminées pour voir si CETTE instance existe déjà
            return CombatManager.Instance.History.CardPlaysFinished.Any(e => 
                e.CardPlay.Card == this &&
                e.CardPlay.Card.Owner == Owner && 
                e.HappenedThisTurn(CombatState)
            );
        }
    }
}