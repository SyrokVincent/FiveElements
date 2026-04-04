using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards.Common;

  
public sealed class MetalCreation() : MetalCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Metal.IsActive(CombatState);
    //Metal: (gain 3 vigor), Gain 1 "metal element"
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(3),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            await CommonActions.ApplySelf<VigorPower>(this, DynamicVars["VigorPower"].BaseValue);
        }

        if (CombatState != null) CombatState.GetElementalStatus().AddEssence(CardElementTag.Metal, 1);
    }

    protected override void OnUpgrade()
    {this.
        AddKeyword(CardKeyword.Innate);
        DynamicVars["VigorPower"].UpgradeValueBy(2);
    }
    /*
    public override async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        GD.Print($"{this} :","on passsssssssssebienla,was",DynamicVars["testelem"].BaseValue);
        // On ne réagit que si c'est l'élément Metal qui change d'état
        if (element == CardElementTag.Metal)
        {
            if (isActive)
            {
                //todo why the fuck is it print 2 times (there is 2 instance of the card maybe it's normal ???
                GD.Print($"{this} :","eleeeeeementchangeToTrue,was",DynamicVars["testelem"].BaseValue);
                DynamicVars["isMetalOn"].BaseValue = 1;
            }
            else
            {
                GD.Print($"{this} :","eleeeeeementchangeTofalse,was",DynamicVars["testelem"].BaseValue);
                DynamicVars["isMetalOn"].BaseValue = 0;
            }
        }

        await Task.CompletedTask;
    }*/
}