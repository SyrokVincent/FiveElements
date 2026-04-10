using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._1_Basic;

public sealed class Activation() : NeutralCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self), IOnElementStateChanged
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    protected override bool ShouldGlowGoldInternal => CombatState != null && FiveElementsCardExtensions.IsAnyElementActive(CombatState);
    
    //Water:(1 energy, 2 wave), Wood:(Draw 1), Fire:(Burn 4 to all enemies), Earth:(6 block), Metal:(3 vigor) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
        new CardsVar(1), 
        new PowerVar<BurnPower>(3),
        new BlockVar(4, ValueProp.Move), 
        new PowerVar<VigorPower>(2),
        new BoolVar("isWaterOn"),
        new BoolVar("isWoodOn"),
        new BoolVar("isFireOn"),
        new BoolVar("isEarthOn"),
        new BoolVar("isMetalOn"),

    ]);
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Water),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Wood),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Earth),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Metal),
        HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
            await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        }
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            await CommonActions.Draw(this, choiceContext);
        }   
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            foreach (var hittableEnemy in CombatState.HittableEnemies)
            {
                await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
            }
        }
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.CardBlock(this, play);
        }
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<VigorPower>(this, DynamicVars["VigorPower"].BaseValue);
        }
    }
    protected override void OnUpgrade()
    {
        //DynamicVars.Energy.UpgradeValueBy(1);
        DynamicVars["WavePower"].UpgradeValueBy(1);
        //DynamicVars.Cards.UpgradeValueBy(1);
        DynamicVars["BurnPower"].UpgradeValueBy(1);
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["VigorPower"].UpgradeValueBy(1);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Fire.IsActive(CombatState))
            {
                return TargetType.AllEnemies;
            }
            return  TargetType.Self;
        }
    } 
    
    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        string? varName = element switch
        {
            CardElementTag.Water => "isWaterOn",
            CardElementTag.Wood => "isWoodOn",
            CardElementTag.Fire => "isFireOn",
            CardElementTag.Earth => "isEarthOn",
            CardElementTag.Metal => "isMetalOn",
            _ => null
        };

        if (varName != null)
        {
            DynamicVars[varName].BaseValue = isActive ? 1 : 0;
        }
        await Task.CompletedTask;
    }
}