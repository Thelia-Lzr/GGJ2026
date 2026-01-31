using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum AIBehaviorType
{
    Aggressive,
    Defensive,
    Balanced,
    Random
}

public class EnemyController : UnitController
{
    [Header("Enemy AI Settings")]
    [SerializeField] protected AIBehaviorType behaviorType = AIBehaviorType.Balanced;
    [SerializeField] protected float thinkDelay = 1f;
    
    protected List<BattleUnit> potentialTargets = new List<BattleUnit>();
    protected Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    
    protected override void Awake()
    {
        base.Awake();
        InitializeResources();
    }
    
    protected virtual void InitializeResources()
    {
        resources[ResourceType.ActionPoint] = 3;
    }
    
    public override bool HasResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            return false;
        
        return resources[type] >= amount;
    }
    
    public override void SpendResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
        {
            resources[type] = 0;
        }
        
        resources[type] = Mathf.Max(0, resources[type] - amount);
    }
    
    public override void GainResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
        {
            resources[type] = 0;
        }
        
        resources[type] += amount;
    }
    
    public override int GetResource(ResourceType type)
    {
        if (!resources.ContainsKey(type))
            return 0;
        
        return resources[type];
    }
    
    public override bool CanPerformAction(ActionCommand command)
    {
        if (!CanAct)
            return false;
        
        if (command == null || !command.IsValid())
            return false;
        
        return HasResource(ResourceType.ActionPoint, command.ResourceCost);
    }
    
    public override void PerformAction(ActionCommand command)
    {
        if (!CanPerformAction(command))
        {
            Debug.LogWarning($"Cannot perform action: {command.ActionType}");
            return;
        }
        
        SpendResource(ResourceType.ActionPoint, command.ResourceCost);
        
        RaiseActionPerformed(command);
    }
    
    public override void TakeTurn()
    {
        if (!CanAct)
        {
            Debug.LogWarning($"Enemy unit {gameObject.name} cannot act this turn.");
            return;
        }
        
        OnTurnStart();
        
        StartCoroutine(ThinkAndAct());
    }
    
    protected System.Collections.IEnumerator ThinkAndAct()
    {
        yield return new WaitForSeconds(thinkDelay);
        
        ActionCommand decision = AI();
        
        if (decision != null && CanPerformAction(decision))
        {
            PerformAction(decision);
            
            if (animationHandler != null)
            {
                System.Collections.IEnumerator actionCoroutine = GetActionCoroutine(decision);
                animationHandler.SubmitAction(actionCoroutine, decision, this);
            }
            else
            {
                Debug.LogWarning("AnimationHandler is not set!");
            }
        }
        
        OnTurnEnd();
    }

    public ActionCommand GetPendingAction()
    {
        // 逻辑：改成这个攻击行为类似读一个列表，像杀戮尖塔这种，不需要AI
        return AI();
    }

    protected virtual System.Collections.IEnumerator GetActionCoroutine(ActionCommand command)
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
    
    public virtual ActionCommand AI()
    {
        RefreshPotentialTargets();
        
        if (potentialTargets.Count == 0)
        {
            return null;
        }
        
        BattleUnit target = SelectTarget();
        
        if (target == null)
        {
            return null;
        }
        
        ActionCommand action = SelectAction(target);
        
        return action;
    }
    
    protected virtual void RefreshPotentialTargets()
    {
        potentialTargets.Clear();
        
        BattleUnit[] allUnits = FindObjectsOfType<BattleUnit>();
        
        foreach (BattleUnit unit in allUnits)
        {
            if (unit.IsAlive() && unit.GetTeam() != boundUnit.GetTeam())
            {
                potentialTargets.Add(unit);
            }
        }
    }
    
    protected virtual BattleUnit SelectTarget()
    {
        if (potentialTargets.Count == 0)
            return null;
        
        switch (behaviorType)
        {
            case AIBehaviorType.Aggressive:
                return SelectLowestHealthTarget();
            
            case AIBehaviorType.Defensive:
                return SelectHighestThreatTarget();
            
            case AIBehaviorType.Balanced:
                return SelectBalancedTarget();
            
            case AIBehaviorType.Random:
                return SelectRandomTarget();
            
            default:
                return SelectRandomTarget();
        }
    }
    
    protected virtual BattleUnit SelectLowestHealthTarget()
    {
        return potentialTargets.OrderBy(t => t.CurrentHealth).FirstOrDefault();
    }
    
    protected virtual BattleUnit SelectHighestThreatTarget()
    {
        return potentialTargets.OrderByDescending(t => t.CurrentHealth).FirstOrDefault();
    }
    
    protected virtual BattleUnit SelectBalancedTarget()
    {
        float avgHealth = (float)potentialTargets.Average(t => t.CurrentHealth);
        return potentialTargets.OrderBy(t => Mathf.Abs(t.CurrentHealth - avgHealth)).FirstOrDefault();
    }
    
    protected virtual BattleUnit SelectRandomTarget()
    {
        int randomIndex = Random.Range(0, potentialTargets.Count);
        return potentialTargets[randomIndex];
    }
    
    protected virtual ActionCommand SelectAction(BattleUnit target)
    {
        List<ActionCommand> availableActions = GetAvailableActions();
        
        if (availableActions.Count == 0)
        {
            return null;
        }
        
        ActionCommand attackCommand = new ActionCommand(this, target, ActionType.Attack)
        {
            ResourceCost = 1
        };
        
        return attackCommand;
    }
    
    public void SetBehaviorType(AIBehaviorType type)
    {
        behaviorType = type;
    }
}
