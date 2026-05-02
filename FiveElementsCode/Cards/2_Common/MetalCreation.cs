using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

  
public sealed class MetalCreation() : MetalCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self,false,false) //removed
{
    protected override bool ShouldGlowGoldInternal => CardElementTag.Metal.IsActive(CombatState);
    
    
    // Gain 3 vigor
    // Metal:( Gain 1 "metal essence")
    //removed innate on upgrade
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Essence,
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>()
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        if (CombatState.GetElementalStatus().GetEssence(CardElementTag.Metal) > 0)
        {
            await CommonActions.ApplySelf<VigorPower>(choiceContext,this, DynamicVars["VigorPower"].BaseValue*2);
        }
        else
        {
            await CommonActions.ApplySelf<VigorPower>(choiceContext,this, DynamicVars["VigorPower"].BaseValue);
            if (CardElementTag.Metal.IsActive(CombatState))
            {
                CombatState.GetElementalStatus().AddEssence(CardElementTag.Metal, 1,choiceContext);
            }
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(2);
    }
    
    //this shit is called before onplay
    //change the pile to exhaust if you have the corresponding essence
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return CombatState != null && (CombatState.GetElementalStatus().GetEssence(CardElementTag.Earth) > 0) ? PileType.Exhaust : resultPileType;
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