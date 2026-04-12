using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class MetalSlash() : MetalCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    //Exhaust, Deal 10 damage, Metal:(if permanently upgrade a random card)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new DamageVar(10, ValueProp.Move),
        new CardsVar(1),
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        //HoverTipFactory.FromPower<WavePower>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    ]);
/*
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        if (CombatState == null || play.Target == null) return;
        
        // Vérifie si les pouvoirs de la cible autorisent le déclenchement du Fatal 
        // (Certains ennemis comme les sbires ne donnent pas de bonus à la mort)
        bool canTriggerFatal = play.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
        
        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(choiceContext);
    

        if (canTriggerFatal && attack.Results.Any(r => r.WasTargetKilled) && CardElementTag.Metal.IsActive(CombatState))
        {
            // On cherche les cartes améliorables dans la version "Run" du deck
            var upgradableInDeck = Owner.Deck.Cards.Where(c => c.IsUpgradable).ToList();
            
            if (upgradableInDeck.Any())
            {
                var cardToUpgrade = upgradableInDeck.TakeRandom(DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardSelection).First();
                
                // CardCmd.Upgrade sur une carte du RunState.Deck est permanent
                CardCmd.Upgrade(cardToUpgrade);
                //CardCmd.Preview(cardToUpgrade); 
                
                // 2. Synchronisation avec le combat actuel
                // On cherche si cette instance précise de carte est actuellement dans une pile de combat
                // (Main, Pioche ou Défausse)
                if (Owner.PlayerCombatState != null)
                {
                    var combatInstance = Owner.PlayerCombatState.AllCards
                        .FirstOrDefault(c => c.DeckVersion == cardToUpgrade);

                    if (combatInstance != null && combatInstance.IsUpgradable)
                    {
                        CardCmd.Upgrade(combatInstance);
                    }
                }
            }
        }

    }
   */


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null || play.Target == null) return;
    
        bool canTriggerFatal = play.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
        bool metalIsActive = CardElementTag.Metal.IsActive(CombatState);
    
        // On utilise l'attaque standard
        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(choiceContext);

        // Si Fatal + Métal Actif
        if (canTriggerFatal && metalIsActive && attack.Results.Any(r => r.WasTargetKilled))
        {
            // On récupère les cartes du Deck (PileType.Deck)
            var upgradableCards = Owner.Deck.Cards.Where(c => c.IsUpgradable).ToList();
        
            if (upgradableCards.Count > 0)
            {
                // Petit délai pour laisser l'ennemi mourir visuellement
                await Cmd.Wait(0.5f);

                // Sélection RNG via la seed de la run
                var cardToUpgrade = Owner.RunState.Rng.Niche.NextItem(upgradableCards);
                if (cardToUpgrade == null) return;

                // --- LA LOGIQUE DE SAUVEGARDE STS2 ---
                // 1. On l'ajoute à l'historique du point actuel sur la carte (indispensable pour la persistance)
                Owner.RunState.CurrentMapPointHistoryEntry?
                    .GetEntry(Owner.NetId).UpgradedCards.Add(cardToUpgrade.Id);

                // 2. On déclenche l'upgrade réel
                cardToUpgrade.UpgradeInternal();
                cardToUpgrade.FinalizeUpgradeInternal();

                // 3. On affiche l'animation visuelle sur l'UI Globale (pour qu'elle reste même si le combat finit)
                if (LocalContext.IsMe(Owner))
                {
                    NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(
                        NCardSmithVfx.Create([cardToUpgrade])!
                    );
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

