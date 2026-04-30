using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class WoodRoots() : WoodCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self), IOnWaterStateChanged
{
    

    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Water.IsActive(CombatState) || CardElementTag.Wood.IsActive(CombatState));

    //Water:(for each energy gained this turn and for every 5 wave, gain 1 surge),
    //Wood:(for every 3 strength gain 1 strength)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isWaterOn"),
        new IntVar("WaveDivider",5),
        new IntVar("StrengthDivider",3),
        new CalculationBaseVar(0), 
        new CalculationExtraVar(1),    
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
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Water),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Wood),
        HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromPower<SurgePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        if (CardElementTag.Water.IsActive(CombatState))
        {
            
            // 1. On récupère le montant actuel de Wave et d'energy gagner
            var currentWave = play.Card.Owner.Creature.GetPowerAmount<WavePower>();
            var tempStrengthToGain = DynamicVars["EnergyGained"].PreviewValue + (currentWave / DynamicVars["WaveDivider"].BaseValue);
            await CommonActions.ApplySelf<SurgePower>(choiceContext,this, tempStrengthToGain);
            
            
        }
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            // 1. On récupère le montant actuel de strength
            var currentStrength = play.Card.Owner.Creature.GetPowerAmount<StrengthPower>();
            var strengthToGain = currentStrength / DynamicVars["StrengthDivider"].BaseValue;
            await CommonActions.ApplySelf<StrengthPower>(choiceContext,this, strengthToGain);
      
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaveDivider"].UpgradeValueBy(-1);
        //DynamicVars["StrengthDivider"].UpgradeValueBy(-1);
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Water) await OnWaterStateChanged(isActive);
        if (element == CardElementTag.Wood) await OnWoodStateChanged(isActive);
    }

    public async Task OnWaterStateChanged(bool isActive)
    {
        DynamicVars["isWaterOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
}