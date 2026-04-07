using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Cards.Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Cards.Rare;

public class WaterVeil() : WaterCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Water.IsActive(CombatState);

    //Reduce damage taken by 25% for 1 turn, Water: (Transform all Status card in your hand into Water Drop)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<WaterVeilPower>(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromCard<WaterDrop>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        
       
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CommonActions.ApplySelf<WaterVeilPower>(this, DynamicVars["WaterVeilPower"].BaseValue);
        if (CardElementTag.Water.IsActive(CombatState))
        {
            var cardsToTransform = PileType.Hand.GetPile(Owner).Cards
                .Where(c => c != null && c.IsTransformable && c.Type == CardType.Status)
                .ToList();
            await TransformInHand<WaterDrop>(Owner,cardsToTransform,IsUpgraded,CombatState);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaterVeilPower"].UpgradeValueBy(1);
    }
    
    
    private static async Task TransformInHand<T>(Player owner, IReadOnlyList<CardModel> cards, bool isUpgraded, CombatState combatState) 
        where T : CardModel // On précise que T doit être un modèle de carte
    {
        foreach (var card in cards )
        {
            var replacementCard = combatState.CreateCard<T>(owner);
        
            // --- FORCER LA MISE À JOUR INITIALE ---
            // On vérifie manuellement chaque élément pour la nouvelle carte
            foreach (CardElementTag elem in Enum.GetValues(typeof(CardElementTag)))
            {
                bool isActive = elem.IsActive(combatState);
                // On appelle la fonction de mise à jour visuelle/logique de la carte
                if (replacementCard is FiveElementsCard elementalCard) 
                {
                    await elementalCard.OnElementStateChanged(elem, isActive);
                }
            }

            //if (isUpgraded) CardCmd.Upgrade(replacementCard);
            
            await CardCmd.Transform(card, replacementCard);
        }

    }
}