using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._2_Common;

public sealed class Distortion() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    // need a few card with strike tag
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    
    //Deal 9, Add 1 Fulu in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9,ValueProp.Move),
        new CardsVar(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Shift,
    ]);
    
    /*
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate),
    ];
    */
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            // 1. On commence par le mot-clé Echo qui est toujours présent
            yield return HoverTipFactory.FromKeyword(FiveElementsKeywords.Shift); //just to put it in front
            yield return HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo);
            yield return HoverTipFactory.FromKeyword(FiveElementsKeywords.Generate);

            // 2. Si on est en combat, on adapte l'aperçu de la carte
            if (CombatState != null)
            {
                // Déterminer quel Fulu afficher dans l'infobulle
                if (Owner.Creature.HasPower<SpiritsFormPower>())
                    yield return HoverTipFactory.FromCard<Fulu>();
                else if (this.CountAsElement(CardElementTag.Water, Owner.Creature))
                    yield return HoverTipFactory.FromCard<WoodFulu>();
                else if (this.CountAsElement(CardElementTag.Wood, Owner.Creature))
                    yield return HoverTipFactory.FromCard<FireFulu>();
                else if (this.CountAsElement(CardElementTag.Fire, Owner.Creature))
                    yield return HoverTipFactory.FromCard<EarthFulu>();
                else if (this.CountAsElement(CardElementTag.Earth, Owner.Creature))
                    yield return HoverTipFactory.FromCard<MetalFulu>();
                else if (this.CountAsElement(CardElementTag.Metal, Owner.Creature))
                    yield return HoverTipFactory.FromCard<WaterFulu>();
                else
                    yield return HoverTipFactory.FromCard<Fulu>();
            }
            else
            {
                // Hors combat (dans le deck run), on affiche le Fulu par défaut
                yield return HoverTipFactory.FromCard<Fulu>();
            }
        }
    }
    
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
        
        if (CombatState != null)
        {
            
            // Si l'Echo a les 5 éléments -> on a tout les element grace au pouvoir spirit form on renvoie un fulu neutre
            if (Owner.Creature.HasPower<SpiritsFormPower>())
            {
                await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
            }
            // Cycle : Eau -> Bois -> Feu -> Terre -> Métal -> Eau
            else if (this.CountAsElement(CardElementTag.Water, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<WoodFulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Wood, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<FireFulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Fire, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<EarthFulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Earth, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<MetalFulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Metal, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<WaterFulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
            else await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, DynamicVars.Cards.IntValue, false, CombatState);
            
        }
        
        
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}