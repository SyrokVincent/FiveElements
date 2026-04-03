using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class WaterBubble() : WaterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self), IOnElementStateChanged
{
    
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);
    
    //Gain 5 block, 2 wave, Water: (cost is 0)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(5, ValueProp.Move),
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
    

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        // On ne réagit que si c'est l'élément Eau qui change d'état
        if (element == CardElementTag.Water)
        {
            if (isActive)
            {
                // L'eau vient de s'activer : on force le coût à 0
                this.EnergyCost.AddThisCombat(-EnergyCost.Canonical);
            }
            else
            {
                // L'eau vient de disparaître : on remet le coût d'origine de la carte
                this.EnergyCost.AddThisCombat(EnergyCost.Canonical);
            }
        }

        await Task.CompletedTask;
    }
}