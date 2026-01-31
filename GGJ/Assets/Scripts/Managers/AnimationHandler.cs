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
            Debug.LogWarning("AnimationHandler: Cannot submit null coroutine.");
            return;
        }
        
        if (executor == null)
        {
            Debug.LogWarning("AnimationHandler: Executor cannot be null, using AnimationHandler as fallback.");
            executor = this;
        }
        
        string commandInfo = command != null ? $"{command.ActionType}" : "null";
        Debug.Log($"AnimationHandler: Submitting action [{commandInfo}] from executor [{executor.gameObject.name}]. Queue size: {actionQueue.Count} -> {actionQueue.Count + 1}");
        
        actionQueue.Enqueue(new ActionQueueItem
        {
            ActionCoroutine = actionCoroutine,
            Command = command,
            Executor = executor
        });
        
        if (!isProcessing)
        {
            Debug.Log("AnimationHandler: Starting queue processing");
            StartCoroutine(ProcessActionQueue());
        }
        else
        {
            Debug.Log("AnimationHandler: Queue already processing, action added to queue");
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
        string commandInfo = item.Command != null ? $"{item.Command.ActionType}" : "null";
        string executorName = item.Executor != null ? item.Executor.gameObject.name : "null";
        
        Debug.Log($"AnimationHandler: [START] Executing action [{commandInfo}] on [{executorName}]");
        
        if (item.Executor == null)
        {
            Debug.LogError($"AnimationHandler: Executor is null for action [{commandInfo}], skipping");
            yield break;
        }
        
        if (item.ActionCoroutine == null)
        {
            Debug.LogError($"AnimationHandler: ActionCoroutine is null for action [{commandInfo}], skipping");
            yield break;
        }
        
        yield return item.Executor.StartCoroutine(item.ActionCoroutine);
        
        Debug.Log($"AnimationHandler: [COMPLETE] Action [{commandInfo}] completed on [{executorName}]");
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
