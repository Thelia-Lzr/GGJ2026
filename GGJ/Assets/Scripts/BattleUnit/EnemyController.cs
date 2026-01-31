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
    [SerializeField] protected AIBehaviorType behaviorType = AIBehaviorType.Random;
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
            ConfirmAction(decision);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 无法执行行动");
        }
        
        // 注意：不再在这里调用 OnTurnEnd()，由 RoundManager 统一管理
    }

    public ActionCommand GetPendingAction()
    {
        // 逻辑：改成这个攻击行为类似读一个列表，像杀戮尖塔这种，不需要AI
        return AI();
    }

    protected override System.Collections.IEnumerator GetActionCoroutine(ActionCommand command)
    {
        switch (command.ActionType)
        {
            case ActionType.Attack:
                attackCount--;
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
        // 获取所有敌对单位（不同阵营）
        List<BattleUnit> enemyUnits = new List<BattleUnit>();
        BattleUnit[] allUnits = FindObjectsOfType<BattleUnit>();
        
        foreach (BattleUnit unit in allUnits)
        {
            // 选择不同阵营的活着的单位
            if (unit.IsAlive() && unit.GetTeam() != boundUnit.GetTeam())
            {
                enemyUnits.Add(unit);
            }
        }
        
        if (enemyUnits.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 没有可攻击的敌对单位");
            return null;
        }
        
        // 随机选择一个敌对单位作为目标
        int randomIndex = Random.Range(0, enemyUnits.Count);
        BattleUnit target = enemyUnits[randomIndex];
        
        Debug.Log($"{gameObject.name} 的 AI 决定攻击敌对单位: {target.gameObject.name}");
        
        // 创建攻击指令
        ActionCommand action = new ActionCommand(this, target, ActionType.Attack);
        
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

        ActionCommand attackCommand = new ActionCommand(this, target, ActionType.Attack);
        
        return attackCommand;
    }
    
    public void SetBehaviorType(AIBehaviorType type)
    {
        behaviorType = type;
    }
}
