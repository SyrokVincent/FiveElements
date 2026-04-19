using FiveElements.FiveElementsCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Godot;
using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Extensions;

namespace FiveElements.FiveElementsCode.Potions;

public sealed class BurnPotion :FiveElementsPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AllEnemies;
    
    // Définition des variables dynamiques (Dégâts affichés dans la description)
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        { 
            yield return new PowerVar<BurnPower>(8);
        }
    }
    
    // Ajoute l'infobulle du Poison automatiquement
    public override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<BurnPower>();
        }
    }
    
    // Logique d'utilisation de la potion
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // On récupère tous les ennemis ciblables
        var creatureCombatState = this.Owner.Creature.CombatState;
        if (creatureCombatState != null)
        {
            IReadOnlyList<Creature> targets = creatureCombatState.HittableEnemies;

            // --- PHASE 1 : Effets Visuels (VFX) ---
            foreach (Creature targetEnemy in targets)
            {
                // On ajoute une explosion de fumée sur chaque ennemi
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(targetEnemy));
            }

            // --- PHASE 2 : Attente ---
            // On attend un petit moment pour que l'explosion soit visible avant les dégâts
            await Cmd.CustomScaledWait(0.2f, 0.3f);

            // --- PHASE 3 : Application des dégâts ---
            // On inflige les dégâts à tous les ennemis
           
            await PowerCmd.Apply<BurnPower>(targets, this.DynamicVars["BurnPower"].BaseValue, this.Owner.Creature, null);
            //await CommonActions.Apply<BurnPower>(hittableEnemy, this, DynamicVars["BurnPower"].BaseValue);
            
        }
    }
    
    
    public override string CustomPackedImagePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }
    
    public override string CustomPackedOutlinePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}

