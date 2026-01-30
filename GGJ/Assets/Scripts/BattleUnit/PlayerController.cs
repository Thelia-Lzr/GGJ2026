using UnityEngine;
using System;
using System.Collections;

public class PlayerController : UnitController
{
    [Header("Player Settings")]
    [SerializeField] private bool waitingForInput = false;
    
    private ActionCommand pendingAction;
    
    public event Action OnTurnStartRequested;
    public event Action<ActionCommand> OnActionConfirmed;
    
    public bool IsWaitingForInput => waitingForInput;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void TakeTurn()
    {
        if (!CanAct)
        {
            Debug.LogWarning($"Player unit {gameObject.name} cannot act this turn.");
            return;
        }
        
        OnTurnStart();
        
        waitingForInput = true;
        
        OnTurnStartRequested?.Invoke();
    }
    
    public void HandleInput(ActionCommand command)
    {
        if (!waitingForInput)
            return;
        
        if (command == null || !CanPerformAction(command))
        {
            Debug.LogWarning("Invalid action command received.");
            return;
        }
        
        pendingAction = command;
    }
    
    public void ConfirmAction(ActionCommand command)
    {
        if (!waitingForInput)
            return;
        
        if (command == null || !CanPerformAction(command))
        {
            Debug.LogWarning("Cannot confirm action.");
            return;
        }
        
        waitingForInput = false;
        
        PerformAction(command);
        
        if (animationHandler != null)
        {
            IEnumerator actionCoroutine = GetActionCoroutine(command);
            animationHandler.SubmitAction(actionCoroutine, command, this);
        }
        else
        {
            Debug.LogWarning("AnimationHandler is not set!");
        }
        
        OnActionConfirmed?.Invoke(command);
    }
    
    protected virtual IEnumerator GetActionCoroutine(ActionCommand command)
    {
        switch (command.ActionType)
        {
            case ActionType.Attack:
                yield return Attack(command.Target);
                break;
            
            case ActionType.SwitchMask:
                if (command.MaskData != null)
                {
                    SwitchMask(command.MaskData, command.ResourceCost);
                }
                break;
            
            default:
                Debug.LogWarning($"Unknown action type: {command.ActionType}");
                break;
        }
    }
    

    public override bool SwitchMask(Mask newMask, int cost)
    {
        bool success = base.SwitchMask(newMask, cost);
        
        if (success)
        {
            Debug.Log($"Player switched to mask: {newMask.MaskName}");
        }
        
        return success;
    }
    
    public void EndTurn()
    {
        if (waitingForInput)
        {
            waitingForInput = false;
        }
        
        OnTurnEnd();
    }
    
    public void CancelAction()
    {
        pendingAction = null;
    }
}
