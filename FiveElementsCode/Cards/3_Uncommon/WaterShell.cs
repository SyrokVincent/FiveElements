using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class WaterShell() : WaterCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(Owner.Creature);

    //Gain 13 block, Water:(Exhaust 1 random status from draw pile and 1 from discard pile)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(13, ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.CardBlock(this, play);
        if (CardElementTag.Water.IsActive(Owner.Creature))
        {
            var status1 = GetStatuses(play.Card.Owner,PileType.Draw).TakeRandom(1,Rng.Chaotic).FirstOrDefault();
            var status2 = GetStatuses(play.Card.Owner,PileType.Discard).TakeRandom(1,Rng.Chaotic).FirstOrDefault();
            
            if (status1 != null) await CardCmd.Exhaust(choiceContext, status1);
            if (status2 != null) await CardCmd.Exhaust(choiceContext, status2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
    
    
    private static IEnumerable<CardModel> GetStatuses(Player owner, PileType pileType)
    {
        return pileType.GetPile(owner).Cards.Where(c => c.Type == CardType.Status);
        // && c.IsTransformable ??? if i change to transform into waterdrop
    }
}