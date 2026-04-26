using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class WoodSurge() : WoodCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Draw 2(3) card and gain 3(5) surge,
    //Wood:(This card is Played when drawn once per turn).
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(2),
        new PowerVar<SurgePower>(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<SurgePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        
        await CommonActions.Draw(this, choiceContext);
        await CommonActions.ApplySelf<SurgePower>(choiceContext,this, DynamicVars["SurgePower"].BaseValue);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
        DynamicVars["SurgePower"].UpgradeValueBy(2);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // On ne réagit que si c'est CETTE carte qui vient d'être piochée
        if (card != this || CombatState == null) return;

        // Conditions : Élément Bois actif ET n'a pas encore été jouée ce tour
        if (CardElementTag.Wood.IsActive(CombatState) && !HasBeenPlayedThisTurn)
        {
            await CardCmd.AutoPlay(choiceContext, this, this.Owner.Creature);
        }
    }

    // LOGIQUE DE VÉRIFICATION DU TOUR
    private bool HasBeenPlayedThisTurn
    {
        get
        {
            // On fouille dans l'historique des cartes terminées pour voir si CETTE instance existe déjà
            return CombatManager.Instance.History.CardPlaysFinished.Any(e => 
                e.CardPlay.Card == this && 
                e.HappenedThisTurn(CombatState)
            );
        }
    }
}