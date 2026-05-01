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

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class EarthMagma() : EarthCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy), IOnFireStateChanged
{

    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Earth.IsActive(CombatState) || CardElementTag.Fire.IsActive(CombatState));

    //Fire:(Gain Block equals to Burn on the enemy),
    //Earth:(Gain 4+1 plating (or thorn?this turn?)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isFireOn"),
        new PowerVar<PlatingPower>(4),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Earth),
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            if (play.Target != null)
            {
                var burnAmount = play.Target.GetPowerAmount<BurnPower>();
                if (burnAmount > 0)
                {
                    await CreatureCmd.GainBlock(Owner.Creature, burnAmount, ValueProp.Move, play);
                }
            }
        }
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<PlatingPower>(choiceContext,this, DynamicVars["PlatingPower"].BaseValue);
        }
    }
    
    

    protected override void OnUpgrade()
    {
        DynamicVars["PlatingPower"].UpgradeValueBy(1);
    }
    
    public override TargetType TargetType 
    {
        get
        {
            if (CardElementTag.Fire.IsActive(CombatState))
            {
                return TargetType.AnyEnemy;
            }
            return  TargetType.Self;
        }
    } 
    
    
    public async Task OnFireStateChanged(bool isActive)
    {
        DynamicVars["isFireOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Fire) await OnFireStateChanged(isActive);
        if (element == CardElementTag.Earth) await OnEarthStateChanged(isActive);
    }
}