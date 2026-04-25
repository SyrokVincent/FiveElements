using FiveElements.FiveElementsCode.Cards._5_Token;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace FiveElements.FiveElementsCode.Powers;

public class IncantationPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Fulu>(),
    ];
    // public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    // {
    // }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner.Player) return;
        if (CombatState != null)
        {
            // just in case I change how echo reseting works
            var currentEcho = Character.FiveElements.Echo;
            // Si l'Echo a les 5 éléments -> on a tout les element grace au pouvoir spirit form on renvoie un fulu neutre
            if (this.HasSpiritsForm)
            {
                await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner.Player, 1, false, combatState);
            }
            // Cycle : Eau -> Bois -> Feu -> Terre -> Métal -> Eau
            else if (currentEcho.TagsCountAsElement(CardElementTag.Water, Owner))
                await FiveElementsCardExtensions.CreateInHand<WoodFulu>(Owner.Player, 1, false, combatState);
        
            else if (currentEcho.TagsCountAsElement(CardElementTag.Wood, Owner))
                await FiveElementsCardExtensions.CreateInHand<FireFulu>(Owner.Player, 1, false, combatState);
        
            else if (currentEcho.TagsCountAsElement(CardElementTag.Fire, Owner))
                await FiveElementsCardExtensions.CreateInHand<EarthFulu>(Owner.Player, 1, false, combatState);
        
            else if (currentEcho.TagsCountAsElement(CardElementTag.Earth, Owner))
                await FiveElementsCardExtensions.CreateInHand<MetalFulu>(Owner.Player, 1, false, combatState);
        
            else if (currentEcho.TagsCountAsElement(CardElementTag.Metal, Owner))
                await FiveElementsCardExtensions.CreateInHand<WaterFulu>(Owner.Player, 1, false, combatState);
            else await FiveElementsCardExtensions.CreateInHand<Fulu>(Owner.Player, 1, false, combatState);
            Flash();
        }
    }
    
}