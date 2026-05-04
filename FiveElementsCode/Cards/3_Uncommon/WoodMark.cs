using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WoodMark() : WoodCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Wood.IsActive(CombatState);

    //Exhaust, remove all thorns and block from the enemy,
    //Wood:(Draw 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        //VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_flying_slash");
        if (play.Target != null)
        {
            // 1. Suppression totale du Block de l'ennemi
            if ( play.Target.Block > 0)
            {
                await CreatureCmd.LoseBlock(play.Target, play.Target.Block);
            }
            // 2. Suppression du thorns 
            if (play.Target.HasPower<ThornsPower>())
            {
                await PowerCmd.Remove<ThornsPower>(play.Target);
            }
        }
        
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            await CommonActions.Draw(this, choiceContext);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}