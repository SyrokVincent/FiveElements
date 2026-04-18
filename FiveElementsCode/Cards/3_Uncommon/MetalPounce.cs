using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class MetalPounce() : MetalCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    //Deal 16 damage, Metal:(vigor apply twice to this attack)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CalculationBaseVar(16), // Dégâts de base
        new ExtraDamageVar(1),    // Dégâts bonus par vigor
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
        {
        
            var metalIsActive = card.CombatState != null && CardElementTag.Metal.IsActive(card.CombatState);

            if (!metalIsActive) return 0;
            
            // On renvoie le multiplicateur (nombre de fois qu'on ajoute ExtraDamageVar)
            return card.Owner.Creature.GetPowerAmount<VigorPower>();
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        await CommonActions.CardAttack(this, play.Target,DynamicVars.CalculatedDamage).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
    }
}