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

public sealed class Isolation() : NeutralCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    //I think it's needed for enchantment?
    public override bool GainsBlock => true;
    //Gain 8 block, Add 1 Fulu(fulu+) in hand
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8,ValueProp.Move),
        new CardsVar(1),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        FiveElementsKeywords.Attune,
    ]);
    
    /*
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(IsUpgraded),
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo),
    ];
    */
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            // 1. On commence par le mot-clé Echo qui est toujours présent
            yield return HoverTipFactory.FromKeyword(FiveElementsKeywords.Attune); // to be in front
            yield return HoverTipFactory.FromKeyword(FiveElementsKeywords.Echo);

            // 2. Si on est en combat, on adapte l'aperçu de la carte
            if (CombatState != null)
            {
                // Déterminer quel Fulu afficher dans l'infobulle
                if (Owner.Creature.HasPower<SpiritsFormPower>())
                    yield return HoverTipFactory.FromCard<Fulu>(IsUpgraded);
                else if (this.CountAsElement(CardElementTag.Water, Owner.Creature))
                    yield return HoverTipFactory.FromCard<WoodFulu>(IsUpgraded);
                else if (this.CountAsElement(CardElementTag.Wood, Owner.Creature))
                    yield return HoverTipFactory.FromCard<FireFulu>(IsUpgraded);
                else if (this.CountAsElement(CardElementTag.Fire, Owner.Creature))
                    yield return HoverTipFactory.FromCard<EarthFulu>(IsUpgraded);
                else if (this.CountAsElement(CardElementTag.Earth, Owner.Creature))
                    yield return HoverTipFactory.FromCard<MetalFulu>(IsUpgraded);
                else if (this.CountAsElement(CardElementTag.Metal, Owner.Creature))
                    yield return HoverTipFactory.FromCard<WaterFulu>(IsUpgraded);
                else
                    yield return HoverTipFactory.FromCard<Fulu>(IsUpgraded);
            }
            else
            {
                // Hors combat (dans le deck run), on affiche le Fulu par défaut
                yield return HoverTipFactory.FromCard<Fulu>(IsUpgraded);
            }
        }
    }
    
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await base.OnPlay(choiceContext, play);
        
        await CommonActions.CardBlock(this, play);
        if (CombatState != null)
        {
            
            // Si l'Echo a les 5 éléments -> on a tout les element grace au pouvoir spirit form on renvoie un fulu neutre
            if (Owner.Creature.HasPower<SpiritsFormPower>())
            {
                await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
            }
            // Cycle : Eau -> Bois -> Feu -> Terre -> Métal -> Eau
            else if (this.CountAsElement(CardElementTag.Water, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<WoodFulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Wood, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<FireFulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Fire, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<EarthFulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Earth, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<MetalFulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
        
            else if (this.CountAsElement(CardElementTag.Metal, Owner.Creature))
                await FiveElementsCardExtensions.CreateInHand<WaterFulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
            else await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner, DynamicVars.Cards.IntValue, IsUpgraded, CombatState);
            
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}