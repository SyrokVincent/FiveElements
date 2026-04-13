using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class EarthBornPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];
    
    
    // Cette méthode empêche la suppression automatique de l'armure.
    // Si c'est notre créature (le joueur), on retourne 'false' pour dire : 
    // "Ne nettoie pas le bloc tout de suite."
    public override bool ShouldClearBlock(Creature creature) => creature != Owner;
    
    // Une fois que le nettoyage standard a été empêché, cette méthode intervient
    public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
    {
        // On vérifie que c'est bien ce pouvoir qui a empêché le nettoyage
        if (preventer != this || creature != Owner)
            return;

        int currentBlock = creature.Block;
        if (currentBlock == 0) return;
        
        await CreatureCmd.LoseBlock(creature, currentBlock/2m);
        Flash();
    }
}