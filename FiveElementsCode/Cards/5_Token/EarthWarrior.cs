using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._5_Token;

[Pool(typeof(TokenCardPool))]
public sealed class EarthWarrior() : EarthCard(1,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    
    public override bool GainsBlock => true;
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(Owner.Creature);
    
    //Gain 3 block,+2 for each Earth guard in your deck, Earth:(Gain x for Earth Guard in your hand)
    // increase to +2 from 1 and now gain thorn if other warrior are in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<ThornsPower>(1),
        new CalculationBaseVar(3), // Base block
        new CalculationExtraVar(2),    // bonus block for each warrior in deck
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
            if (card.Owner?.PlayerCombatState == null) return 0;

            // Calcul du nombre de guerriers dans tout le deck (non exilés)
            var totalWarriors = card.Owner.PlayerCombatState.AllCards
                .Count(c => c is EarthWarrior && c.Pile?.Type != PileType.Exhaust);
        
            var bonusCount = totalWarriors;

            // Si l'élément Terre est actif, on ajoute ceux en main //replaced with thorn
         /*   if (card.CombatState != null && CardElementTag.Earth.IsActive(card.CombatState))
            {
                var warriorsInHand = PileType.Hand.GetPile(card.Owner).Cards
                    .Count(c => c is EarthWarrior);
            
                bonusCount += (warriorsInHand); //double the value if in hand ?
            }*/
            
            // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
            return bonusCount;
        })
    ]);

   
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.FromCard<EarthWarrior>(), //flavor \o/
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        //await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.PreviewValue,DynamicVars.CalculatedBlock.Props, play);
        //await CommonActions.CardBlock(this, DynamicVars.CalculatedBlock, play);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.PreviewValue,
            ValueProp.Unpowered, play); //preview + unpowered to apply exactly waht's shown , might be wrong ??
        
       if (CardElementTag.Earth.IsActive(Owner.Creature))
       {
           var warriorsInHand = PileType.Hand.GetPile(Owner).Cards
               .Count(c => c is EarthWarrior);
           
           await CommonActions.ApplySelf<ThornsPower>(choiceContext,this, warriorsInHand*DynamicVars["ThornsPower"].BaseValue);

       }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3);
    }
}