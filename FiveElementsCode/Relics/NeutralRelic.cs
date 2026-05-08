using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class NeutralRelic() : FiveElementsRelic
{
    //The first time a turn you played a card of each element gain 1 strengh and 1 dexterity
    public override RelicRarity Rarity => RelicRarity.Shop;
    
    
    // On stocke les éléments déjà joués ce tour pour éviter les doublons
    private readonly SortedSet<CardElementTag> _elementsPlayedThisTurn = new();
    private int _activationsThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<StrengthPower>(1),
        new PowerVar<DexterityPower>(1),
    ]);

 
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ]); 
    

    private int ActivationsThisTurn
    {
        get => _activationsThisTurn;
        set
        {
            this.AssertMutable();
            _activationsThisTurn = value;
            this.Status = _activationsThisTurn > 0 ? RelicStatus.Active : RelicStatus.Normal;
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
            return Task.CompletedTask;

        // Reset au début du tour
        _elementsPlayedThisTurn.Clear();
        ActivationsThisTurn = 0;
        return Task.CompletedTask;
    }

    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (ActivationsThisTurn >= 1 || cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress)
            return;

        var elementStatus = Owner.Creature.GetElementalStatus();
        // 1. On cherche d'abord si la carte a stocké ses tags au moment du OnPlay (avant le Shift/Attune)
        //je comprend pas trop pourquoi les carte avec shift change d'element la... (c'est car la relic est apres la starter relic qui gere echo? et les pouvoir marche sans ça car il trigger avant les relic ?)
        if (cardPlay.Card is NeutralCard && elementStatus.PlayedElementsCache.TryGetValue(cardPlay, out var capturedTags))
        {
            // On utilise notre extension TagsCountAsElement pour gérer SpiritsForm 
            // sur les tags qui étaient présents à ce moment-là.
            if (capturedTags.TagsCountAsElement(CardElementTag.Water, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Water);
            if (capturedTags.TagsCountAsElement(CardElementTag.Wood, Owner.Creature))  _elementsPlayedThisTurn.Add(CardElementTag.Wood);
            if (capturedTags.TagsCountAsElement(CardElementTag.Fire, Owner.Creature))  _elementsPlayedThisTurn.Add(CardElementTag.Fire);
            if (capturedTags.TagsCountAsElement(CardElementTag.Earth, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Earth);
            if (capturedTags.TagsCountAsElement(CardElementTag.Metal, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Metal);
        }
        else 
        {
            // 2. Si ce n'est pas une NeutralCard, on utilise la détection classique
            if (cardPlay.Card.CountAsElement(CardElementTag.Water, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Water);
            if (cardPlay.Card.CountAsElement(CardElementTag.Wood, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Wood);
            if (cardPlay.Card.CountAsElement(CardElementTag.Fire, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Fire);
            if (cardPlay.Card.CountAsElement(CardElementTag.Earth, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Earth);
            if (cardPlay.Card.CountAsElement(CardElementTag.Metal, Owner.Creature)) _elementsPlayedThisTurn.Add(CardElementTag.Metal);
        }

        // --- DEBUG ---
        //string currentElements = string.Join(", ", _elementsPlayedThisTurn);
        //GD.Print($"[NeutralRelic] Capture pour : {cardPlay.Card.Id.Entry} | Stock : [{currentElements}]");

        if (_elementsPlayedThisTurn.Count >= 5)
        {
            this.Flash();
            await PowerCmd.Apply<StrengthPower>(context, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, null);
            await PowerCmd.Apply<DexterityPower>(context, Owner.Creature, DynamicVars["DexterityPower"].BaseValue, Owner.Creature, null);
            ActivationsThisTurn++;
        }
    }
    
    
    
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        _elementsPlayedThisTurn.Clear();
        ActivationsThisTurn = 0;
        return Task.CompletedTask;
    }

    
}
