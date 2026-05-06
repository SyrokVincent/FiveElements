using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class WaterSource() : WaterCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(Owner.Creature);

    //Gain 7(9) wave, Water: (cost 1 less) 
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WavePower>(7),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WavePower"].UpgradeValueBy(2);
    }
    
    private bool _isWaterDiscountActive = false;

    public override async Task OnWaterStateChanged(bool isActive, Creature creature)
    {
        await base.OnWaterStateChanged(isActive, creature);
        // Si l'eau s'active ET que la réduction n'est pas encore appliquée
        if (isActive && !_isWaterDiscountActive)
        {
            this.EnergyCost.AddThisCombat(-1);
            _isWaterDiscountActive = true;
        }
        // Si l'eau se désactive ET que la réduction était appliquée
        else if (!isActive && _isWaterDiscountActive)
        {
            this.EnergyCost.AddThisCombat(1);
            _isWaterDiscountActive = false;
        }
        await Task.CompletedTask;
    }
}