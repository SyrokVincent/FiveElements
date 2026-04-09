using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class WaterDrop() : WaterCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);

    // Exhaust, Gain 1 wave? Water: (Gain 1 energy)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new EnergyVar(1),
        //new PowerVar<WavePower>(0),
        
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
       // HoverTipFactory.FromPower<WavePower>()
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust, 
    ];
    

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        //await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        if (CardElementTag.Water.IsActive(CombatState))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        //todo an upgrade either draw or few wave
        //DynamicVars["WavePower"].UpgradeValueBy(2);
    }
}