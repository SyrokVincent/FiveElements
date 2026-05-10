using BaseLib.Extensions;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace FiveElements.FiveElementsCode.Powers;

public sealed class MindAndBodyAttunementMindPower : FiveElementsPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override object? InitInternalData() => new MindAndBodyAttunementMindData();
    public MindAndBodyAttunementMindData GetData() => GetInternalData<MindAndBodyAttunementMindData>();

    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ally = cardPlay.Card.Owner;

        // Si l'allié (pas nous) a le pouvoir de lien
        if (ally.Creature != Owner && ally.HasPower<MindAndBodyAttunementBodyPower>())
        {
            // On capture NOTRE Echo actuel pour l'associer à SA carte
            var myCurrentEcho = Owner.GetElementalStatus().Echo;
            
            // On fige l'état : Neutral + notre Echo
            GetData().CapturedElements[cardPlay] = [CardElementTag.Neutral, ..myCurrentEcho];
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // Reset au début de notre tour
        if (player != Owner.Player) return;
        GetData().CapturedElements.Clear();
        await PowerCmd.Decrement(this);
    }


    public class MindAndBodyAttunementMindData
    {
        // On associe l'ID de la carte jouée par l'allié aux éléments qu'elle a capturés chez toi
        public Dictionary<CardPlay, SortedSet<CardElementTag>> CapturedElements = new();
    }
}
