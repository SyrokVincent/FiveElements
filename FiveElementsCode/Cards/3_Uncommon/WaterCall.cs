using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WaterCall() : WaterCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{

    //delete if shouldn't glow
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);

    //Deal 3 damage, Gain 2 Wave, Water: (trigger wave) 

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(3,ValueProp.Move),
        new PowerVar<WavePower>(2),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<WavePower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
        await CommonActions.ApplySelf<WavePower>(this, DynamicVars["WavePower"].BaseValue);
        if (CardElementTag.Water.IsActive(CombatState))
        {
            // trigger wave
            // On récupère l'instance du pouvoir sur le joueur
            var wavePower = Owner.Creature.GetPower<WavePower>();
            
            //todo decide if the target is the front or if it's the same as the attack
            if (wavePower != null && play.Target != null && CombatState != null)
                    await wavePower.TriggerWave(CombatState, play.Target, choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["WavePower"].UpgradeValueBy(1);
    }
}