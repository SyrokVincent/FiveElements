using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class WoodSurge() : WoodCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //gain 1 strength, 1 for every 3 energy gained this turn ,
    //Wood:(Deal 1 to a random enemies for each strength)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new IntVar("EnergyDivider",3), // divise la strengt bonus
        new DamageVar(1,ValueProp.Move),
        new CalculationBaseVar(0), // Dégâts de base
        new CalculationExtraVar(1),    // strength bonus 
        new CalculatedVar("EnergyGained").WithMultiplier((card, target) =>
        {
            if (card.CombatState == null) 
                return 0;
            
            // 1. CALCUL DE L'ENERGIE DÉPENSÉE via l'historique
            // On somme le coût payé de toutes les cartes terminées ce tour-ci
            var energySpent = CombatManager.Instance.History.Entries
                .OfType<EnergySpentEntry>() // Vérifie le nom exact de l'entry (souvent CardPlayedEntry ou CardPlayFinishedEntry)
                .Where(e => e.HappenedThisTurn(card.CombatState) && e.Actor.Player == card.Owner)
                .Sum(e => e.Amount);

            if (card.Owner.PlayerCombatState == null) return 0;
            
            var currentEnergy = card.Owner.PlayerCombatState.Energy;
    
            var totalEnergyGained = (currentEnergy + energySpent);
            totalEnergyGained = Math.Max(0, totalEnergyGained);

            return totalEnergyGained;

        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<StrengthPower>(),
    ]);

    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        var strengthToGain = DynamicVars["EnergyGained"].PreviewValue / DynamicVars["EnergyDivider"].BaseValue;
        if (strengthToGain > 0)
        {
            await CommonActions.ApplySelf<StrengthPower>(choiceContext,this, strengthToGain);
        }
        
        if (CardElementTag.Wood.IsActive(CombatState))
        {

            var currentStrength = Owner.Creature.GetPowerAmount<StrengthPower>();
            if (currentStrength > 0)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(currentStrength)
                    .FromCard(this)
                    .TargetingRandomOpponents(CombatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyDivider"].UpgradeValueBy(-1);
    }
    
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Wood.IsActive(CombatState))
            {
                return TargetType.RandomEnemy;
            }
            return  TargetType.Self;
        }
    }
    
}