using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class WoodRelic() : FiveElementsRelic
{
    //Deal 5 damage to random enemy at end of turn if you used wood card this turn
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(5,ValueProp.Move), 
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    private bool _isActivating;
    private int WoodPlayedThisTurn { get; set; }

  
    public override Task BeforeCombatStart()
    {
        WoodPlayedThisTurn = 0;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // On vérifie si la carte jouée possède l'élément Wood

        if (cardPlay.Card is NeutralCard && NeutralCard.PlayedElementsCache.TryGetValue(cardPlay, out var capturedTags))
        {
            if (capturedTags.TagsCountAsElement(CardElementTag.Wood, Owner.Creature))
            {
                if (WoodPlayedThisTurn == 0)
                {
                    WoodPlayedThisTurn = 1;
                    Status = RelicStatus.Active;
                    this.Flash();
                }
            }
        }else if (cardPlay.Card.CountAsElement(CardElementTag.Wood, Owner.Creature))
        {
            if (WoodPlayedThisTurn == 0)
            {
                WoodPlayedThisTurn = 1;
                Status = RelicStatus.Active;
                this.Flash();
            }
        }
        return Task.CompletedTask;
    }

    public override async Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player && WoodPlayedThisTurn > 0)
        {
            
            if (Owner.Creature.CombatState != null)
            {
                var target = Owner.RunState.Rng.CombatTargets.NextItem(Owner.Creature.CombatState.HittableEnemies);
                if (target != null)
                {
                    _ = DoActivateVisuals();
                    await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage, Owner.Creature);
                }
            }
            
            WoodPlayedThisTurn = 0;
            Status = RelicStatus.Normal;
        }
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // Sécurité : on reset au début du tour au cas où
        WoodPlayedThisTurn = 0;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
    
    private async Task DoActivateVisuals()
    {
        _isActivating = true;
        this.Flash();
        UpdateDisplay();
        
        await Cmd.Wait(1f); // Petit délai pour l'animation
        
        _isActivating = false;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_isActivating)
        {
            Status = RelicStatus.Normal;
        }
        else
        {
            Status = (WoodPlayedThisTurn == 1) 
                ? RelicStatus.Active 
                : RelicStatus.Normal;
        }
        InvokeDisplayAmountChanged();
    }
}
