using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards.Basic;

public class Activation() : FiveElementsCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    //Wood:(Draw 1), Fire:(Burn 4 to all enemies), Earth:(6 block), Metal:(3 vigor), Water:(1 energy, 2 wave)
    protected override HashSet<CardElementTag> CanonicalElementTags => [CardElementTag.Neutral];
    
    protected override bool ShouldGlowGoldInternal => IsAnyElementActive();
    
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1), 
        new EnergyVar(1), 
        new BlockVar(6, ValueProp.Move), 
        new PowerVar<VigorPower>(3)
        /*  new BoolVar("water_on",IsElementActive(CardElementTag.Water)),
       new BoolVar("wood_on",IsElementActive(CardElementTag.Wood)),
       new BoolVar("fire_on",IsElementActive(CardElementTag.Fire)),
       new BoolVar("earth_on",IsElementActive(CardElementTag.Earth)),
       new BoolVar("metal_on",IsElementActive(CardElementTag.Metal)),*/
        
        // does not find how to update the color once set so give up on it for now
        // new StringVar("water_s",IsElementActive(CardElementTag.Water) ? "" : "[color=#666666]"),
        // new StringVar("water_e",IsElementActive(CardElementTag.Water) ? "" : "[/color]"),
        // new StringVar("wood_s",IsElementActive(CardElementTag.Wood) ? "" : "[color=#666666]"),
        // new StringVar("wood_e",IsElementActive(CardElementTag.Wood) ? "" : "[/color]"),
        // new StringVar("fire_s",IsElementActive(CardElementTag.Fire) ? "" : "[color=#666666]"),
        // new StringVar("fire_e",IsElementActive(CardElementTag.Fire) ? "" : "[/color]"),
        // new StringVar("earth_s",IsElementActive(CardElementTag.Earth) ? "" : "[color=#666666]"),
        // new StringVar("earth_e",IsElementActive(CardElementTag.Earth) ? "" : "[/color]"),
        // new StringVar("metal_s",IsElementActive(CardElementTag.Metal) ? "" : "[color=#666666]"),
        // new StringVar("metal_e",IsElementActive(CardElementTag.Metal) ? "" : "[/color]"),

    ]);
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (IsElementActive(CardElementTag.Water))
        {
            await PlayerCmd.GainEnergy( DynamicVars.Energy.IntValue, Owner);
            //also nneed to add wave
        }
        if (IsElementActive(CardElementTag.Wood))
        {
            await CommonActions.Draw(this, choiceContext);
        }   
        if (IsElementActive(CardElementTag.Fire))
        {
            // add burn to all enemies (draw for now to see effect)
            await CommonActions.Draw(this, choiceContext);
        }
        if (IsElementActive(CardElementTag.Earth))
        {
            await CommonActions.CardBlock(this, play);
        }
        if (IsElementActive(CardElementTag.Metal))
        {
            await CommonActions.ApplySelf<VigorPower>(this, DynamicVars["VigorPower"].IntValue);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["VigorPower"].UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
    
}