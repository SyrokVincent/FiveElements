using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Interfaces;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class FireBlossom() : FireCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies), IOnWoodStateChanged
{
    private readonly Color _vfxTint = new Color("ff6347"); //tomato red
    
    
    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        (CardElementTag.Wood.IsActive(CombatState) || CardElementTag.Fire.IsActive(CombatState));

    //Wood:(For every strength, apply 3 burn to a random enemy),
    //Fire:(Burn not removed 1 time)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BoolVar("isWoodOn"),
        new PowerVar<BurnPower>(3),
        new PowerVar<FireBlossomPower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Wood),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Fire),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (CardElementTag.Wood.IsActive(CombatState))
        {
            Vector2 lastPos = Vector2.Zero;
            // 1. On récupère le montant actuel de strength
            var currentStrength = play.Card.Owner.Creature.GetPowerAmount<StrengthPower>();
            for (int i = 0; i < currentStrength; i++)
            {
                // Sélection d'un ennemi aléatoire
                Creature? enemy = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);

                if (enemy != null)
                {
                    /* //was wayway too long with too much strength
                    if (TestMode.IsOff)
                    {
                        // Gestion des effets visuels (VFX)
                        if (i == 0)
                            lastPos = NCombatRoom.Instance.GetCreatureNode(Owner.Creature).VfxSpawnPosition;

                        var targetNode = NCombatRoom.Instance.GetCreatureNode(enemy);
                        if (targetNode != null)
                        {
                            //todo replace poisonpotion with something cool
                            var vfx = NItemThrowVfx.Create(lastPos, targetNode.GetBottomOfHitbox(),
                                ModelDb.Potion<PoisonPotion>().Image);
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);

                            lastPos = targetNode.VfxSpawnPosition;
                            await Cmd.Wait(0.5f);

                            // Effets d'impact
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(
                                NSplashVfx.Create(targetNode.VfxSpawnPosition, _vfxTint));
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(
                                NLiquidOverlayVfx.Create(enemy, _vfxTint));
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(
                                NGaseousImpactVfx.Create(targetNode.VfxSpawnPosition, _vfxTint));
                        }
                    }*/

                    // Application du burn
                    await PowerCmd.Apply<BurnPower>(choiceContext,enemy, DynamicVars["BurnPower"].BaseValue, Owner.Creature, this);
                }
            }
        }
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            var targets = CombatState.HittableEnemies;
            await PowerCmd.Apply<FireBlossomPower>(choiceContext,targets, this.DynamicVars["FireBlossomPower"].BaseValue, this.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPower"].UpgradeValueBy(2);
    }
    

    public async Task OnWoodStateChanged(bool isActive)
    {
        DynamicVars["isWoodOn"].BaseValue = isActive ? 1 : 0;
        await Task.CompletedTask;
    }

    public async Task OnElementStateChanged(CardElementTag element, bool isActive)
    {
        if (element == CardElementTag.Fire) await OnFireStateChanged(isActive);
        if (element == CardElementTag.Wood) await OnWoodStateChanged(isActive);
    }
}