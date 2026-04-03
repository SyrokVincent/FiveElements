using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Powers;
using FiveElements.FiveElementsCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using static FiveElements.FiveElementsCode.Extensions.FiveElementsCardExtensions;

namespace FiveElements.FiveElementsCode.Cards.Basic;

public sealed class Activation() : NeutralCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.AllEnemies)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && IsAnyElementActive(CombatState );
    
    //Water:(1 energy, 2 wave), Wood:(Draw 1), Fire:(Burn 4 to all enemies), Earth:(6 block), Metal:(3 vigor) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1), 
        new PowerVar<WavePower>(2),
        new CardsVar(1), 
        new PowerVar<BurnPower>(3),
        new BlockVar(4, ValueProp.Move), 
        new PowerVar<VigorPower>(2),
        /*  new BoolVar("water_on",IsElementActive(CardElementTag.Water)),
       new BoolVar("wood_on",IsElementActive(CardElementTag.Wood)),
       new BoolVar("fire_on",IsElementActive(CardElementTag.Fire)),
       new BoolVar("earth_on",IsElementActive(CardElementTag.Earth)),
       new BoolVar("metal_on",IsElementActive(CardElementTag.Metal)),*/
        
        // does not find how to update the color once set so give up on it for now
        // new StringVar("water_s",IsElementActive(CardElementTag.Water) ? "" : "[color=#666666]"),
        // new StringVar("water_e",IsElementActive(CardElementTag.Water) ? "" : "[/color]"),
        // new StringVar("wood_s",IsElementActive(CardElementTag.Wood) ? "" : "[color=#666666]"),
        // new StringVar("wood_e",IsElementActive(CardElementTag.Wood) ? "" : "[/color]"),
        // new StringVar("fire_s",IsElementActive(CardElementTag.Fire) ? "" : "[color=#666666]"),
        // new StringVar("fire_e",IsElementActive(CardElementTag.Fire) ? "" : "[/color]"),
        // new StringVar("earth_s",IsElementActive(CardElementTag.Earth) ? "" : "[color=#666666]"),
        // new StringVar("earth_e",IsElementActive(CardElementTag.Earth) ? "" : "[/color]"),
        // new StringVar("metal_s",IsElementActive(CardElementTag.Metal) ? "" : "[color=#666666]"),
        // new StringVar("metal_e",IsElementActive(CardElementTag.Metal) ? "" : "[/color]"),

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
    
}