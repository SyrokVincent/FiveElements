using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards._1_Basic;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public class Relic2() : StarterRelicLogic
{
    
    public override RelicRarity Rarity => RelicRarity.Starter;
    
    //First turn add Creation+ in hand. Trigger corresponding effect on activation when you gain essence
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        //need that if i ever want it to draw 2 lol
        //new CardsVar("Draw",1),
        
        //water
        ActivationVars.Energy,
        ActivationVars.Wave,
        //wood
        ActivationVars.Cards,
        ActivationVars.TempStrength,
        //fire
        ActivationVars.Burn,
        //earth
        ActivationVars.Block,
        //metal
        ActivationVars.Vigor,
    ]);
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<Creation>(true),
        HoverTipFactory.FromCard<Activation>(),
    ]); 
    
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        await base.BeforeHandDraw(player,choiceContext, combatState);
        
        // On vérifie si c'est le premier tour
        if (player == Owner && player.Creature.CombatState is { RoundNumber: 1 })
        {
            //ajout de la carte
            await FiveElementsCardExtensions.CreateInHand<Creation>(Owner, 1,true, combatState);
            await Task.CompletedTask;
        }
    }
    // --- ABONNEMENT À L'ÉLÉMENT ---

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        if (Owner.Creature.CombatState != null)
            Owner.Creature.CombatState.GetElementalStatus().EssenceChanged += OnEssenceGainedTrigger;
        await Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        if (Owner.Creature.CombatState != null)
            Owner.Creature.CombatState.GetElementalStatus().EssenceChanged -= OnEssenceGainedTrigger;
        await Task.CompletedTask;
    }

    

    // --- LA LOGIQUE DE LA RELIQUE ---
    private void OnEssenceGainedTrigger(CardElementTag elem, int newValue, PlayerChoiceContext? context = null)
    {
        if (newValue <= 0) return;

        // On lance la Task sans l'attendre (Fire and Forget)
        // On utilise _ = pour indiquer au compilateur qu'on ignore volontairement le retour
        _ = TriggerRelicEffect(elem, context);
    }

    private async Task TriggerRelicEffect(CardElementTag elem, PlayerChoiceContext? context)
    {
        this.Flash();

        switch (elem)
        {
            
            case CardElementTag.Water:
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
                await PowerCmd.Apply<WavePower>(Owner.Creature, DynamicVars["WavePower"].BaseValue, Owner.Creature, null);
                break;
            
           
            case CardElementTag.Wood:
                if (context != null) await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue, Owner);
                await PowerCmd.Apply<ActivationTempStrengthPower>(Owner.Creature, DynamicVars["ActivationTempStrengthPower"].BaseValue, Owner.Creature, null);
                break;
            
            case CardElementTag.Fire:
                if (Owner.Creature.CombatState != null)
                {
                    var targets = Owner.Creature.CombatState.HittableEnemies;
                    await PowerCmd.Apply<BurnPower>(targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, null);
                }

                break;
            
            case CardElementTag.Earth:
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, DynamicVars.Block.Props, null);
                break;
            
            case CardElementTag.Metal:
                await PowerCmd.Apply<VigorPower>(Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, null);
                break;
        }
    }

}