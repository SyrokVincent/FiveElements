using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public class Cycle() : NeutralCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    
    //Deal 7 damage, draw 3 card, only keep the echo card
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(7,ValueProp.Move),
        new CardsVar(3),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Attune,
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null || play.Target == null) return;

        
        // 1. Attaque
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // Pioche 3 cartes
        var drawnCards = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        // Filtrage : On garde les cartes Echo, on défausse le reste
        var cardsToDiscard = drawnCards.Where(card => !card.CountsAsElement(Character.FiveElements.Echo,Owner.Creature));
        
        
        await CardCmd.Discard(choiceContext, cardsToDiscard);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
    
}