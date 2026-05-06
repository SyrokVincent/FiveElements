using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Interfaces;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;

namespace FiveElements.FiveElementsCode.Hooks;

public static class MyModHooks
{
    /*
    public static async Task TriggerElementStateChanged(IRunState runState, CombatState? combatState, CardElementTag element, bool isActive)
    {
        if (combatState == null) return;
        
        foreach (var listener in runState.IterateHookListeners(combatState))
        {
            if (listener is IOnElementStateChanged elementListener)
            {
                // On transmet les infos reçues
                await elementListener.OnElementStateChanged(element, isActive);
                listener.InvokeExecutionFinished();
            }
        }
    }*/
    
    
    public static async Task TriggerElementStateChanged(IRunState runState, ICombatState combatState, CardElementTag element, bool isActive, Creature creature)
    {
        if (combatState == null) return;
    
        foreach (var listener in runState.IterateHookListeners(combatState))
        {
            // On ne vérifie que l'interface parente
            if (listener is IOnElementStateChanged elementListener)
            {
                await elementListener.OnElementStateChanged(element, isActive, creature);
                listener.InvokeExecutionFinished();
            }
        }
    }
}