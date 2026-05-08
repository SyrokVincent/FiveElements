using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards;
public abstract class FireCard : FiveElementsCard, IOnFireStateChanged
{
    
    public override Material? CreateCustomFrameMaterial => FireShader;
    
    protected FireCard(int cost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true, bool autoAdd = true) 
        : base(cost, type, rarity, target, showInCardLibrary, autoAdd)
    {
        CanonicalElementTags = [CardElementTag.Fire];
    }   
    
    public async Task OnFireStateChanged(bool isActive, Creature creature)
    {
        if (Owner.Creature != creature) return;
        DynamicVars["isFireOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isFireOn"),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    //need to overide for card that don't have Element:
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ]);
    
    
    protected async Task DealHeatDamage(PlayerChoiceContext choiceContext, Creature? target, CalculatedDamageVar damage)
    {
        ArgumentNullException.ThrowIfNull(target);

        // Exécution de l'attaque
        var attackResult = await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

         
        // On boucle sur les résultats pour appliquer le Burn individuellement
        foreach (var result in attackResult.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0)
            {
                // result.Target est la créature qui a reçu les dégâts
                await CommonActions.Apply<BurnPower>(choiceContext, result.Receiver, this, result.UnblockedDamage);
            }
        }
    }
    
    protected async Task DealHeatDamage(PlayerChoiceContext choiceContext, Creature? target, DamageVar damageVar)
    {
        // Sécurité : Vérifie que la cible existe
        ArgumentNullException.ThrowIfNull(target);

        // Exécution de l'attaque via le DamageCmd
        var attackResult = await DamageCmd.Attack(damageVar.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        
        // On boucle sur les résultats pour appliquer le Burn individuellement
        foreach (var result in attackResult.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0)
            {
                // result.Target est la créature qui a reçu les dégâts
                await CommonActions.Apply<BurnPower>(choiceContext, result.Receiver, this, result.UnblockedDamage);
            }
        }
    }
    
    
    protected async Task DealHeatDamageAoe(PlayerChoiceContext choiceContext, CalculatedDamageVar damage)
    {
        // Exécution de l'attaque
        if (CombatState != null)
        {
            var attackResult = await DamageCmd.Attack(damage)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);

         
            // On boucle sur les résultats pour appliquer le Burn individuellement
            foreach (var result in attackResult.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0)
                {
                    // result.Target est la créature qui a reçu les dégâts
                    await CommonActions.Apply<BurnPower>(choiceContext, result.Receiver, this, result.UnblockedDamage);
                }
            }
        }
    }
    
    
    protected async Task DealHeatDamageAoe(PlayerChoiceContext choiceContext, DamageVar damageVar)
    {

        // Exécution de l'attaque via le DamageCmd
        if (CombatState != null)
        {
            var attackResult = await DamageCmd.Attack(damageVar.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);

        
            // On boucle sur les résultats pour appliquer le Burn individuellement
            foreach (var result in attackResult.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0)
                {
                    // result.Target est la créature qui a reçu les dégâts
                    await CommonActions.Apply<BurnPower>(choiceContext, result.Receiver, this, result.UnblockedDamage);
                }
            }
        }
    }
    
}