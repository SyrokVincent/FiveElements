using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WaterFlow() : WaterCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(Owner.Creature);
    
    //Gain 3 energy, gain 2 wave, Water:(next 1(2) turn add water drop in hand)
    // removed draw 1, added 2 wave
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(3),
        //new CardsVar(1),
        new PowerVar<WavePower>(2),
        new PowerVar<WaterDropNextTurnPower>(1),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.FromPower<WaterDropNextTurnPower>(),
        HoverTipFactory.FromCard<WaterDrop>(),
    ]);


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
        //await CommonActions.Draw(this, choiceContext);
        await CommonActions.ApplySelf<WavePower>(choiceContext,this, DynamicVars["WavePower"].BaseValue);
        if (CardElementTag.Water.IsActive(Owner.Creature))
        {
            await CommonActions.ApplySelf<WaterDropNextTurnPower>(choiceContext,this, DynamicVars["WaterDropNextTurnPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaterDropNextTurnPower"].UpgradeValueBy(1);
    }
}