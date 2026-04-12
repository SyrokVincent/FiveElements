using BaseLib.Utils;
using FiveElements.FiveElementsCode.Cards;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace FiveElements.FiveElementsCode.Cards._4_Rare;

public class MetalRefinement() : MetalCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private const string IncreaseKey = "Increase";
    private int _currentVigor = 1;
    private int _increasedVigor;
    
    // Cette propriété est sauvegardée dans le fichier de run (.sav)
    [SavedProperty]
    public int CurrentVigor
    {
        get => _currentVigor;
        set
        {
            AssertMutable();
            _currentVigor = value;
            DynamicVars["VigorPower"].BaseValue = _currentVigor;
        }
    }
    
    [SavedProperty]
    public int IncreasedVigor
    {
        get => _increasedVigor;
        set
        {
            AssertMutable();
            _increasedVigor = value;
        }
    }
    
    protected override bool ShouldGlowGoldInternal => CombatState != null && CardElementTag.Metal.IsActive(CombatState);

    //Exhaust, Gain 1 vigor, Metal:(permanentaly increase this card vigor by 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat([
        new PowerVar<VigorPower>(CurrentVigor),
        new IntVar(IncreaseKey, 1)
    ]);

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Concat([
        CardKeyword.Exhaust, 
    ]);

    //gain echo and elem: description, remove concat if I don't want them
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([
        HoverTipFactory.FromPower<VigorPower>(),
    ]);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // On gagne le vigor actuel
        await CommonActions.ApplySelf<VigorPower>(this, CurrentVigor);
        
        if (CardElementTag.Metal.IsActive(CombatState))
        {
            // On récupère la valeur d'augmentation
            int extra = DynamicVars[IncreaseKey].IntValue;

            // On améliore la carte actuelle (celle en combat)
            BuffFromPlay(extra);

            // Crucial : On améliore AUSSI la carte qui restera dans le deck après le combat
            if (DeckVersion is MetalRefinement deckCard)
            {
                deckCard.BuffFromPlay(extra);
            }
        }
       
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IncreaseKey].UpgradeValueBy(1);
    }
    
    protected override void AfterDowngraded() => UpdateVigor();

    private void BuffFromPlay(int extraBlock)
    {
        IncreasedVigor += extraBlock;
        UpdateVigor();
    }

    private void UpdateVigor() => CurrentVigor = 1 + IncreasedVigor;
}