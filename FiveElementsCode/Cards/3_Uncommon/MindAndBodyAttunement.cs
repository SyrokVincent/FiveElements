using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class MindAndBodyAttunement() : NeutralCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    //Exhaust, This turn, gain [gold]Mind attuned[/gold] and give an ally [gold]Body attuned[/gold].
    // upgrade could remove exhaust, increase turn count or add retain
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<MindAndBodyAttunementMindPower>(1),
        new PowerVar<MindAndBodyAttunementBodyPower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Attune,
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Attune),
        HoverTipFactory.FromPower<MindAndBodyAttunementMindPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        if (CombatState == null) return;

        if (play.Target != null)
        {
            await PowerCmd.Apply<MindAndBodyAttunementMindPower>(choiceContext, Owner.Creature,this.DynamicVars["MindAndBodyAttunementMindPower"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<MindAndBodyAttunementBodyPower>(choiceContext, play.Target,this.DynamicVars["MindAndBodyAttunementBodyPower"].BaseValue, Owner.Creature, this);
        }
            
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Exhaust);
    }
}