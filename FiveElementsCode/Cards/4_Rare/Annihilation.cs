using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class Annihilation() : NeutralCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy), IOnElementStateChanged
{


    protected override bool ShouldGlowGoldInternal => CombatState != null && FiveElementsCardExtensions.IsAnyElementActive(CombatState);

    // Wood:(Deal 15), Fire:(Deal 15), Earth:(Deal 15), Metal:(Deal 15), Water:(Deal 15)
    //added shift on upgrade, buffed base dmg to 15
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(15,ValueProp.Move),
        new BoolVar("isWaterOn"),
        new BoolVar("isWoodOn"),
        new BoolVar("isFireOn"),
        new BoolVar("isEarthOn"),
        new BoolVar("isMetalOn"),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Element),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate)
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;
        int count = 0;
        if (CardElementTag.Water.IsActive(CombatState))
        {
            count++;
        }
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            count++;
        }   
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            count++;
        }
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            count++;
        }
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            count++;
        }
        if (count!=0)
        {
            await CommonActions.CardAttack(this, play.Target,count).Execute(choiceContext);
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        AddKeyword(FiveElementsKeywords.Shift);
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