using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._3_Uncommon;

public sealed class EarthJewel() : EarthCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    
    public override bool GainsBlock => true;
    
    //delete if shouldn't glow or replace water
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //If you took no damage last turn, put this into your hand, Earth:(Gain 3 block)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(3,ValueProp.Move)
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;
        if (CardElementTag.Earth.IsActive(CombatState))
        {
            await CommonActions.CardBlock(this, play);
        }
    }
    
    
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        // Si ce n'est pas le tour du propriétaire, on ne fait rien
        // Si on est au premier tour, il n'y a pas de "tour précédent", donc on ignore.
        if (player != Owner || CombatState == null || CombatState.RoundNumber <= 1) return;
        
        // On vérifie s'il existe une entrée de dégâts reçus :
        // 1. Qui s'est produite au tour PRÉCÉDENT (TurnIndex - 1)
        // 2. Dont la cible était le joueur (Owner.Creature)
        // 3. Où les dégâts ont traversé le blocage (UnblockedDamage > 0)
        var tookDamageLastTurn = CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Any(e => e.RoundNumber == CombatState.RoundNumber - 1 && 
                      e.Receiver == Owner.Creature && 
                      e.Result.UnblockedDamage > 0);

        // Si on n'a PAS pris de dégâts (tookDamageLastTurn est false)
        if (!tookDamageLastTurn && Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
    
    

}