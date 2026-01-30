using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private static AnimationHandler instance;
    
    public static AnimationHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AnimationHandler>();
                
                if (instance == null)
                {
                    Debug.LogError("AnimationHandler instance not found in scene! Please add an AnimationHandler to the scene.");
                }
            }
            return instance;
        }
    }
    
    private Queue<ActionQueueItem> actionQueue = new Queue<ActionQueueItem>();
    private bool isProcessing = false;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Multiple AnimationHandler instances detected! Destroying duplicate on {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    private class ActionQueueItem
    {
        public IEnumerator ActionCoroutine { get; set; }
        public ActionCommand Command { get; set; }
        public MonoBehaviour Executor { get; set; }
    }
    
    public void SubmitAction(IEnumerator actionCoroutine, ActionCommand command, MonoBehaviour executor)
    {
        if (actionCoroutine == null)
        {
            Debug.LogWarning("Cannot submit null coroutine.");
            return;
        }
        
        if (executor == null)
        {
            Debug.LogWarning("Executor cannot be null, using AnimationHandler as fallback.");
            executor = this;
        }
        
        actionQueue.Enqueue(new ActionQueueItem
        {
            ActionCoroutine = actionCoroutine,
            Command = command,
            Executor = executor
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
        Debug.Log($"AnimationHandler: Executing action {item.Command.ActionType} on {item.Executor.gameObject.name}");
        
        yield return item.Executor.StartCoroutine(item.ActionCoroutine);
        
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
