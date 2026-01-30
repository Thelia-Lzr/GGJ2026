using UnityEngine;
using System;

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
        
        OnActionConfirmed?.Invoke(command);
    }
    
    public virtual void Attack(BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid attack target.");
            return;
        }
        
        ActionCommand attackCommand = new ActionCommand(this, target, ActionType.Attack)
        {
            ResourceCost = 1
        };
        
        ConfirmAction(attackCommand);
    }
    
    public virtual void UseSkill(Skill skill, BattleUnit target)
    {
        if (skill == null || !skill.CanUse(this))
        {
            Debug.LogWarning("Cannot use skill.");
            return;
        }
        
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid skill target.");
            return;
        }
        
        ActionCommand skillCommand = new ActionCommand(this, target, ActionType.Skill)
        {
            SkillData = skill,
            ResourceCost = 1
        };
        
        ConfirmAction(skillCommand);
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
