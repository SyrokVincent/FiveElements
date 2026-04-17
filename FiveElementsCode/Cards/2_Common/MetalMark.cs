using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class MetalMark() : MetalCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    
    //todo find a way to preview damage as if opponent already as vulnerable ? maybe with previewValue or with calcultatedVar/damage
    //Apply 1 vulnerable, Metal:(Deal 6 damage(or gain vigor?))
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VulnerablePower>(1),
        new DamageVar(6,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VulnerablePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            await CommonActions.Apply<VulnerablePower>(play.Target, this, DynamicVars["VulnerablePower"].BaseValue);
            if (CardElementTag.Metal.IsActive(CombatState))
            {
                await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VulnerablePower"].UpgradeValueBy(1);
    }
}