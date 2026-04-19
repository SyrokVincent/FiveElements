using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class WaterBubble() : WaterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);
    
    //Gain 4 block, 2 wave, Water: (cost is 0)
    // block from 5 to 4
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(4, ValueProp.Move),
        new PowerVar<WavePower>(2),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]);
    
   

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["WavePower"].UpgradeValueBy(1);
    }
    
    
    private bool _isWaterDiscountActive = false;

    public override async Task OnWaterStateChanged(bool isActive)
    {
        await base.OnWaterStateChanged( isActive);
        // Si l'eau s'active ET que la réduction n'est pas encore appliquée
        if (isActive && !_isWaterDiscountActive)
        {
            this.EnergyCost.AddThisCombat(-this.EnergyCost.Canonical);
            _isWaterDiscountActive = true;
        }
        // Si l'eau se désactive ET que la réduction était appliquée
        else if (!isActive && _isWaterDiscountActive)
        {
            this.EnergyCost.AddThisCombat(this.EnergyCost.Canonical);
            _isWaterDiscountActive = false;
        }
        await Task.CompletedTask;
    }
    
}