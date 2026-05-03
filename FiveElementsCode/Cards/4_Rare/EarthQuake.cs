using BaseLib.Utils;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public sealed class EarthQuake() : EarthCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Earth.IsActive(CombatState);

    //Gain 14 block, Remove half your block to deal that much damage to all enemies,
    //Earth:(Replay this card if it kill an enemy.)
    //
    // moveed to uncommon and reduce block by 2// reverted that and increased by 2
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new BlockVar(14,ValueProp.Move),
        new CalculationBaseVar(0M),
        new ExtraDamageVar(1M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) => 
        {
            // 1. On récupère le bloc actuel du joueur
            decimal currentBlock = (decimal)card.Owner.Creature.Block;

            // 2. On récupère le bloc que la carte VA donner (Dex et buffs inclus)
            decimal blockFromCard = (decimal)card.DynamicVars.Block.PreviewValue;

            // 3. On fait le calcul sur le total futur
            // si 15 bloc, fera 7 dmg
            return Math.Floor((currentBlock + blockFromCard) / 2m);
        })
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
    ]);
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState == null) return;

        // 1. Gain de bloc de base
        await CommonActions.CardBlock(this, play);

        // 2. Calcul des dégâts (Moitié du bloc TOTAL après le gain ci-dessus)
        var currentBlock = Owner.Creature.Block;
        var damageAmount = Math.Floor(currentBlock / 2m);

        // si 15 block, on enleve 7 et on garde 8 block
        if (damageAmount > 0)
        {
            
            // 3. Retrait de la moitié du bloc
            await CreatureCmd.LoseBlock(Owner.Creature, damageAmount);
            
            // 4. Attaque de zone
            var attackAction = CommonActions.CardAttack(this, play.Target,  damageAmount);
            await attackAction.Execute(choiceContext);

            // --- EFFET EARTH (FATAL: REPLAY) ---
            if (CardElementTag.Earth.IsActive(CombatState))
            {
                // On vérifie si l'attaque a tué au moins une cible
                if (attackAction.Results.Any(r => r.WasTargetKilled))
                {
                    // On utilise CardCmd.autoPlay pour rejouer la carte gratuitement
                    await CardCmd.AutoPlay(choiceContext, play.Card, null);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}