using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class WoodLeaf() : WoodCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //First time a turn you have no wood card, put this into your hand, Wood:(Deal 3)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(3,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            await CommonActions.CardAttack(this,play.Target).Execute(choiceContext);;
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Wood.IsActive(CombatState))
            {
                return TargetType.AnyEnemy;
            }
            return  TargetType.Self;
        }
    }

    // Variable pour s'assurer que ça ne se déclenche qu'une fois par tour
    private int _lastTriggerTurn = -1;
    // empeche que la carte reviennent directement en main a chaque fois lorsque une carte est autoplay before hand draw
    private bool _canComebackToHand = false;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _canComebackToHand = true;
        await CheckAndReturnToHand(choiceContext);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CheckAndReturnToHand(context);
    }

    
    private async Task CheckAndReturnToHand(PlayerChoiceContext choiceContext)
    {
        if (CombatState == null || !_canComebackToHand) return;
        
        // 1. Si CETTE graine est déjà en main ou a déjà trigger, on stop.
        if (Pile?.Type == PileType.Hand || _lastTriggerTurn == CombatState.RoundNumber) return;

        // 2. On vérifie s'il y a DU BOIS en main
        bool hasAnyWoodInHand = PileType.Hand.GetPile(Owner).Cards
            .Any(c => c.CountAsElement(CardElementTag.Wood,Owner.Creature));

        // 3. SI LA MAIN EST VIDE :
        if (!hasAnyWoodInHand)
        {
            
            // On récupère TOUTES les WoodSeeds qui ne sont pas en main (dans la pioche ou défausse)
            if (Owner.PlayerCombatState != null)
            {
                var allSeedsInDeck = Owner.PlayerCombatState.AllCards
                    .OfType<WoodLeaf>()
                    .Where(c => c.Pile?.Type != PileType.Hand && c._lastTriggerTurn != CombatState.RoundNumber)
                    .ToList();

                // On les fait toutes revenir d'un coup
                foreach (var seed in allSeedsInDeck)
                {
                    seed._lastTriggerTurn = CombatState.RoundNumber;
                    await CardPileCmd.Add(seed, PileType.Hand);
                }
            }
        }
    }

    public override Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player) _canComebackToHand = false;
        return Task.CompletedTask;
    }
}