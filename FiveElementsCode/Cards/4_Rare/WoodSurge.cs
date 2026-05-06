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

public sealed class WoodSurge() : WoodCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(Owner.Creature);

    //Draw 2(3) card and gain 3 surge,
    //Wood:(This card is Played the first time it is drawn this turn).
    // upgrade no longer increase surge gained by 2
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
        //DynamicVars["SurgePower"].UpgradeValueBy(2);
    }

    
    private bool _justEnteredHand = false;

    
    // --- LOGIQUE D'AUTO-PLAY CENTRALISÉE ---
    private async Task TryAutoPlay(PlayerChoiceContext choiceContext)
    {
        if (CombatState == null || Owner?.Creature == null) return;

        // On vérifie toutes les conditions
        if (_justEnteredHand && 
            CardElementTag.Wood.IsActive(Owner.Creature) && 
            !HasBeenPlayedThisTurn)
        {
            // IMPORTANT : On consomme le flag AVANT l'autoplay
            // Cela empêche AfterCardPlayedLate et AfterCardDrawn de se marcher dessus
            _justEnteredHand = false; 
            
            await CardCmd.AutoPlay(choiceContext, this, Owner.Creature);
        }
    }

    // Pour les potions et effets de pioche directs
    // work when you draw the card and you have wood active before the card that make you draw
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this || CombatState == null) return;
        
        // On marque qu'elle vient d'entrer (sécurité si AfterCardChangedPilesLate n'est pas passé)
        _justEnteredHand = true; 
        await TryAutoPlay(choiceContext);
    }

    // Pour les cartes qui font piocher ET activent le bois en même temps
    //need that for when the card that make you draw also activate wood
    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TryAutoPlay(choiceContext);
        
        // On ferme la fenêtre de tir à la fin de n'importe quelle carte jouée
        _justEnteredHand = false;
    }

    // On garde AfterCardChangedPilesLate pour capturer l'entrée en main proprement
    public override Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card == this && oldPileType == PileType.Draw && card.Pile?.Type == PileType.Hand)
        {
            _justEnteredHand = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return; 
        _justEnteredHand = false;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // On s'assure que si elle était dans la main de départ, 
        // elle ne se considère pas comme "venant d'être piochée" pour le reste du tour
        _justEnteredHand = false;
        return Task.CompletedTask;
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