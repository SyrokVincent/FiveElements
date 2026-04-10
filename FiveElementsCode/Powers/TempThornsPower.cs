using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Powers;

public class TempThornsPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // On ne réagit que si le porteur reçoit des dégâts d'un attaquant valide
        // Les épines se déclenchent sur les attaques (IsPoweredAttack) ou sur Omnislice
        if (target != Owner || dealer == null) return;
        if (!props.IsPoweredAttack() && cardSource is not Omnislice) return;

        Flash();

        // On renvoie les dégâts (Amount d'épines)
        // Note : ValueProp.Unpowered empêche les épines de déclencher d'autres épines à l'infini
        await CreatureCmd.Damage(
            choiceContext, 
            dealer, 
            Amount, 
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, 
            Owner, 
            cardSource: null
        );
    }

    // --- Suppression au début du tour ---
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        // On ne retire le pouvoir que si c'est bien le tour du propriétaire
        if (player == Owner.Player)
        {
            // On retire complètement le pouvoir
            await PowerCmd.Remove(this);
        }
    }
}