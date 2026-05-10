using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Character;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace FiveElements.FiveElementsCode.Relics;

[Pool(typeof(FiveElementsRelicPool))]
public sealed class Relic3() : FiveElementsRelic
{
    // select an element from a card in deck, the relic will give it's essence on combat start
    // removed every 5 turn
    public override RelicRarity Rarity => RelicRarity.Common;
    
    public override bool HasUponPickupEffect => true;
    
    private CardElementTag _myElementTag = CardElementTag.Neutral;
    
    // [SavedProperty] permet de sauvegarder cette valeur dans le fichier .autosave
    [SavedProperty]
    public CardElementTag MyElementTag
    {
        get => _myElementTag;
        set
        {
            this.AssertMutable(); // Sécurité StS2 pour éviter les modifs hors combat/chargement
            _myElementTag = value;
        }
    }
    
    
    private bool _isActivating;
    private int _counter;
    
    public int Counter
    {
        get => _counter;
        set
        {
            this.AssertMutable(); // Sécurité StS2 pour éviter les modifs hors combat/chargement
            _counter = value;
            this.InvokeDisplayAmountChanged();
        }
    }
    
    
    
    public override LocString Title
    {
        get
        {
            // 1. On récupère le titre de base (ex: "Soul")
            LocString title = new LocString("relics", $"{Id.Entry}.title");

            if (MyElementTag != CardElementTag.Neutral)
            {
                string elementKey = MyElementTag.ToString().ToUpperInvariant();
            
                // On cherche "ELEMENT-FIRE" dans le JSON
                LocString elementTitle = new LocString("relics", $"ELEMENT-{elementKey}");
            
                // On combine : "Fire soul"
                elementTitle.Add(nameof(Title), title);
                title = elementTitle;
            }

            if (IsWax)
            {
                LocString prefix = ToyBox.WaxRelicPrefix;
                prefix.Add(nameof(Title), title);
                return prefix;
            }

            return title;
        }
    }
    
    /* // I'm just hiding the counter for if I ever need it back
    // Affiche le compteur seulement si un combat est en cours
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    
    //affichage (0, 1, 2)
    public override int DisplayAmount => _isActivating 
        ? DynamicVars["Turns"].IntValue 
        : Counter;
    */
    
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DynamicVar("Essence",1), 
        new DynamicVar("Turns",5), 
        new IntVar("Element",(int) MyElementTag),
    ]);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Essence),
    ]); 
    
     
    public override async Task AfterObtained()
    {
        await ShowSelectionScreen(this.Owner);
    }
    
    public override Task BeforeCombatStart()
    {
        // On commence à turn - 1 pour que le tour 1 soit le premier incrément
        //Counter = DynamicVars["Turns"].IntValue-1;
        Counter = -1;
        DynamicVars["Element"].BaseValue = (int)MyElementTag;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
    
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        await base.BeforeHandDraw(player, choiceContext, combatState);
        
        if (MyElementTag == CardElementTag.Neutral) return;

        // just in case I need it back the counter
        int maxTurns = DynamicVars["Turns"].IntValue;
        // Incrémentation et modulo
        //Counter = (Counter + 1) % maxTurns;
        Counter = (Counter + 1);

        // Si on est au tour précédant le bonus, on fait briller la relique
        Status = (Counter == maxTurns - 1) ? RelicStatus.Active : RelicStatus.Normal;

        // Si le compteur revient à 0, on donne l'énergie
        if (Counter == 0)
        {
            _ = DoActivateVisuals();
            Owner.Creature.GetElementalStatus().AddEssence(MyElementTag, DynamicVars["Essence"].IntValue, choiceContext);
        }
        
    }
    
    private async Task DoActivateVisuals()
    {
        _isActivating = true;
        this.Flash();
        await Cmd.Wait(1f);
        _isActivating = false;
        this.InvokeDisplayAmountChanged();
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

   

    private async Task ShowSelectionScreen(Player player)
    {
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1)
        {
            RequireManualConfirmation = false,
        };

        // 2. Appeler FromDeckGeneric avec un filtre
        // On suppose que tu as une extension ou une propriété .GetElement() sur tes CardModel
        var selectedCards = await CardSelectCmd.FromDeckGeneric(
            player,
            prefs,
            filter: card => card.IsRemovable && card is FiveElementsCard feCard && !feCard.IsElement(CardElementTag.Neutral)
        );

        // 3. Récupérer la carte sélectionnée
        var selectedCard = selectedCards?.FirstOrDefault();

        if (selectedCard is FiveElementsCard f)
        {
            this.Flash();
            MyElementTag = f.ElementTags.FirstOrDefault(); 
            DynamicVars["Element"].BaseValue = (int)MyElementTag;
            //GD.Print($"[Relic] Élément de relic3 choisi : {MyElementTag}");
            
            //remove the card from the deck
            await CardPileCmd.RemoveFromDeck(f);
        }
    }
    
    /*
    private async Task ShowSelectionScreen(Player player)
    {
        
        
        // 1. Préparer les préférences
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1)
        {
            RequireManualConfirmation = false,
        };
        
        // 1. Créer les options (des cartes temporaires qui servent de boutons)
        // Note : On utilise player.PlayerCombatState si on est en combat, 
        // ou simplement les modèles de cartes si on est hors combat.
        var choices = new List<CardModel>
        {
            ModelDb.Card<WaterSpirit>(),
            ModelDb.Card<WoodSpirit>(),
            ModelDb.Card<FireSpirit>(),
            ModelDb.Card<EarthSpirit>(),
            ModelDb.Card<MetalSpirit>()
        };
        
        // 2. Ouvrir l'écran de sélection
        // Comme on est hors combat on utilise un contexte de blocage??
        var selectedCard = await CardSelectCmd.FromDeckGeneric( //FromChooseACardScreen(
            choices,
            player,
            prefs
        );

        // 3. Appliquer l'effet définitif
        if (selectedCard != null)
        {
            this.Flash();
            if (selectedCard is WaterSpirit) 
            {
                _myElementTag = CardElementTag.Water;
            }else if (selectedCard is WoodSpirit)
            {
                _myElementTag = CardElementTag.Wood;
            }else if (selectedCard is FireSpirit)
            {
                _myElementTag = CardElementTag.Fire;
            }else if (selectedCard is EarthSpirit) 
            {
                _myElementTag = CardElementTag.Earth;
            }else if (selectedCard is MetalSpirit) 
            {
                _myElementTag = CardElementTag.Metal;
            }
        }
    }
    */
    
}
