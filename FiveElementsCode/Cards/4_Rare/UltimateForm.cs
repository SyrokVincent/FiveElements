using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class UltimateForm() : NeutralCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    
    //When you play cards in the generate order, upgrade them and trigger the corresponding effect on the card Activation.
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        //HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
        HoverTipFactory.FromCard<Activation>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        
        if (CombatState == null) return;
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<UltimateFormPower>(this, 1);

    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1);
    }
}