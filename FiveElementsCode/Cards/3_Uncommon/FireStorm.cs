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
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public class FireStorm() : FireCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Fire.IsActive(CombatState);

    //Deal 5 Heat damage to ALL enemies. At turn start play from the exhaust pile when you end a turn with Fire Echo, Fire:(Exhaust itself)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(5,ValueProp.Move),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromKeyword(FiveElementsKeywords.Heat),
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            await DealHeatDamage(choiceContext, hittableEnemy, DynamicVars.Damage);
        }
        if (CardElementTag.Fire.IsActive(CombatState))
        {
            //await CardCmd.Exhaust(choiceContext, this);
            //use of getpile instead, for fireball to count correctly
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    //this shit is called before onplay
    //change the pile to exhaust if fire is active
    protected override PileType GetResultPileType()
    {
        PileType resultPileType = base.GetResultPileType();
        return (CardElementTag.Fire.IsActive(CombatState)) ? PileType.Exhaust : resultPileType;
    }

    /*
    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState)
    {
        // 1. Vérifie si la carte est dans ta pile d'épuisement
        if (Pile?.Type == PileType.Exhaust && player == Owner)
        {
            // 2. On cherche la dernière carte jouée par le joueur au tour precedent
            var lastPlaylastTurn = CombatManager.Instance.History.CardPlaysStarted
                .LastOrDefault(e => e.CardPlay.Card.Owner == Owner && e.RoundNumber == combatState.RoundNumber -1);

            // 3. Filtres stricts :
            // - L'historique ne doit pas être vide
            // - La carte doit être de type Feu
            bool wasFireEchoLastTurn = lastPlaylastTurn != null 
                                       && lastPlaylastTurn.CardPlay.Card.CountsAsElement(CardElementTag.Fire,Owner.Creature);

            if (wasFireEchoLastTurn)
            {
                await CardCmd.AutoPlay(choiceContext, this, null);
            }
        }
    }*/
    
    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        CombatState combatState)
    {
        // 1. Sécurité : La carte doit être en Exhaust et appartenir au joueur
        if (Pile?.Type != PileType.Exhaust || player != Owner)
            return;

        // 2. On cherche la TOUTE DERNIÈRE carte jouée au tour précédent
        var lastEntry = CombatManager.Instance.History.CardPlaysStarted
            .LastOrDefault(e => e.RoundNumber == (combatState.RoundNumber - 1) && e.Actor.Player == player);

        if (lastEntry == null) return;

        // 3. Vérification de l'élément FEU via le cache
        bool wasFire = false;
    
        // On vérifie d'abord dans le cache des tags figés
        if (NeutralCard.PlayedElementsCache.TryGetValue(lastEntry.CardPlay, out var frozenTags))
        {
            wasFire = frozenTags.TagsCountAsElement(CardElementTag.Fire, Owner.Creature);
        }
        else
        {
            // Fallback pour les cartes pas neutres et de base sans cache
            wasFire = lastEntry.CardPlay.Card.CountsAsElement(CardElementTag.Fire, Owner.Creature);
        }

        if (wasFire)
        {
            // On joue la carte automatiquement. 
            await CardCmd.AutoPlay(choiceContext, this, null);
        }
    }
}