using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Queue<ActionQueueItem> actionQueue = new Queue<ActionQueueItem>();
    private bool isProcessing = false;
    
    private class ActionQueueItem
    {
        public IEnumerator ActionCoroutine { get; set; }
        public ActionCommand Command { get; set; }
    }
    
    public void SubmitAction(IEnumerator actionCoroutine, ActionCommand command)
    {
        if (actionCoroutine == null)
        {
            Debug.LogWarning("Cannot submit null coroutine.");
            return;
        }
        
        actionQueue.Enqueue(new ActionQueueItem
        {
            ActionCoroutine = actionCoroutine,
            Command = command
        });
        
        if (!isProcessing)
        {
            StartCoroutine(ProcessActionQueue());
        }
    }
    
    private IEnumerator ProcessActionQueue()
    {
        isProcessing = true;
        
        while (actionQueue.Count > 0)
        {
            ActionQueueItem item = actionQueue.Dequeue();
            
            yield return StartCoroutine(ExecuteAction(item));
        }
        
        isProcessing = false;
    }
    
    private IEnumerator ExecuteAction(ActionQueueItem item)
    {
        Debug.Log($"AnimationHandler: Executing action {item.Command.ActionType}");
        
        yield return StartCoroutine(item.ActionCoroutine);
        
        Debug.Log($"AnimationHandler: Action {item.Command.ActionType} completed");
    }
    
    public void HandleAttackDrag(BattleUnit attacker, BattleUnit target)
    {
    }
    
    public void HandleMaskEquipDrag(BattleUnit target, Mask mask)
    {
    }
    
    public void PlayAttackAnimation(UnitController attacker, BattleUnit target)
    {
    }
    
    public void PlayEquipMaskAnimation(BattleUnit target, Mask mask)
    {
    }
    
    public void OnHitFrame(UnitController attacker, BattleUnit target, ActionCommand command)
    {
    }
    
    public void OnEquipFrame(BattleUnit target, Mask mask)
    {
    }
    
    public void OnAnimationComplete()
    {
    }
}
