using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class EarthClay() : EarthCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{

    //public override bool GainsBlock => true;

    // Glow Rouge : S'active s'il n'y a aucune autre carte de Bloc en main
    protected override bool ShouldGlowRedInternal => 
        CombatState != null && 
        !PileType.Hand.GetPile(Owner).Cards.Any(c => c.GainsBlock && c != this);


    // on ne glow Gold que si l'élément est actif ET qu'il y a du bloc à copier
    protected override bool ShouldGlowGoldInternal => 
        CombatState != null && 
        CardElementTag.Earth.IsActive(Owner.Creature) && 
        PileType.Hand.GetPile(Owner).Cards.Any(c => c.GainsBlock && c != this);
    
   
    //Choose a Block card to gain it's block, Earth:(transform it into Earth Warrior, it cost 1 more this turn)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<EarthWarrior>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null) return;
        // 1. Préparer les préférences de sélection
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

        // 2. Sélectionner une carte qui génère du Bloc
        var selection = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            prefs, 
            c => c.GainsBlock && c != this, // On évite de se choisir soi-même
            this
        );
        var cardModels = selection.ToList();
        var selectedCard = cardModels.FirstOrDefault();
        if (selectedCard == null) return;
        
        // 3. LOGIQUE DE BASE : Gagner le bloc de la carte sélectionnée
        // On utilise PreviewStats pour récupérer la valeur de bloc actuelle de la carte (avec bonus de dextérité, etc.)
        
        decimal blockToGain = 0;

        // On vérifie de manière sécurisée si la clé existe dans le dictionnaire
        if (selectedCard.DynamicVars.ContainsKey("Block")) 
        {
            blockToGain = selectedCard.DynamicVars.Block.PreviewValue;
        }
        if (selectedCard.DynamicVars.TryGetValue("CalculatedBlock", out var block))
        {
            blockToGain += block.PreviewValue;
        }

        if (blockToGain > 0)
        {
            //todo unpowered pour compter les buff une seule fois, je sais pas quelle ordre est le mieux
            await CreatureCmd.GainBlock(Owner.Creature, blockToGain, ValueProp.Unpowered, play);
        }
        
        
        if (CardElementTag.Earth.IsActive(Owner.Creature))
        {
            if (CombatState != null)
            {
                //await FiveElementsCardExtensions.TransformInHand<EarthWarrior>(Owner,cardModels,selectedCard.IsUpgraded,CombatState);
                
                // 1. On crée le Warrior manuellement pour pouvoir modifier son coût
                var warrior = CombatState.CreateCard<EarthWarrior>(Owner);
                if (selectedCard.IsUpgraded) 
                    CardCmd.Upgrade(warrior);
                warrior.EnergyCost.AddThisTurnOrUntilPlayed(1);
                await FiveElementsCardExtensions.TransformInHand(selectedCard, warrior, false,Owner.Creature);
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
    
}