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
    protected AIBehaviorType behaviorType = AIBehaviorType.Random;
    protected float thinkDelay = 1f;
    
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
            
            case ActionType.ActivateMask:
                if (command.MaskData != null)
                {
                    Debug.Log($"[EnemyController] 执行面具启效果: {command.MaskData.MaskName}");
                    yield return command.MaskData.Activate(this);
                }
                else
                {
                    Debug.LogWarning("[EnemyController] ActivateMask 命令缺少 MaskData");
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
    
    // ==================== 攻击方法 ====================
    
    /// <summary>
    /// 单体攻击 - 攻击单个目标
    /// </summary>
    protected System.Collections.IEnumerator AttackSingle(BattleUnit target, float damageMultiplier = 1f)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Enemy] AttackSingle: 目标无效或已死亡");
            yield break;
        }

        Debug.Log($"[Enemy] {gameObject.name} 发动单体攻击，目标: {target.gameObject.name}");

        Vector2 originalPosition = transform.position;
        
        yield return MoveToTarget(target, 0.3f);
        
        int damage = Mathf.RoundToInt(boundUnit.Attack * damageMultiplier);
        target.ApplyHealthChange(-damage);
        Debug.Log($"[Enemy] {gameObject.name} 对 {target.gameObject.name} 造成 {damage} 点伤害");
        
        yield return new WaitForSeconds(0.1f);
        
        yield return MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 群体攻击 - 对所有敌方单位造成伤害
    /// </summary>
    protected System.Collections.IEnumerator AttackAOE(BattleUnit primaryTarget, float damageMultiplier = 1f)
    {
        if (boundUnit == null)
        {
            Debug.LogWarning("[Enemy] AttackAOE: BoundUnit 无效");
            yield break;
        }

        Team enemyTeam = boundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetAllUnitsOfTeam(enemyTeam);

        if (enemies.Count == 0)
        {
            Debug.LogWarning("[Enemy] AttackAOE: 没有找到敌方单位");
            yield break;
        }

        Debug.Log($"[Enemy] {gameObject.name} 发动群体攻击，目标数: {enemies.Count}");

        Vector2 originalPosition = transform.position;
        
        if (primaryTarget != null && primaryTarget.IsAlive())
        {
            yield return MoveToTarget(primaryTarget, 0.3f);
        }
        
        int damage = Mathf.RoundToInt(boundUnit.Attack * damageMultiplier);
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                enemy.ApplyHealthChange(-damage);
                Debug.Log($"[Enemy] {gameObject.name} 对 {enemy.gameObject.name} 造成 {damage} 点伤害");
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 溅射攻击 - 对目标及相邻位置的敌人造成伤害
    /// </summary>
    protected System.Collections.IEnumerator AttackSplash(BattleUnit target, float damageMultiplier = 1f)
    {
        if (boundUnit == null)
        {
            Debug.LogWarning("[Enemy] AttackSplash: BoundUnit 无效");
            yield break;
        }

        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Enemy] AttackSplash: 主目标无效或已死亡");
            yield break;
        }

        Team enemyTeam = target.UnitTeam;
        Location targetLocation = target.UnitLocation;
        List<BattleUnit> splashTargets = new List<BattleUnit>();

        splashTargets.Add(target);

        List<Location> adjacentLocations = GetAdjacentLocations(targetLocation);
        List<BattleUnit> allEnemies = GetAllUnitsOfTeam(enemyTeam);

        foreach (var enemy in allEnemies)
        {
            if (enemy != target && adjacentLocations.Contains(enemy.UnitLocation) && enemy.IsAlive())
            {
                splashTargets.Add(enemy);
            }
        }

        Debug.Log($"[Enemy] {gameObject.name} 发动溅射攻击，主目标: {target.gameObject.name}，溅射目标数: {splashTargets.Count}");

        Vector2 originalPosition = transform.position;
        
        yield return MoveToTarget(target, 0.3f);
        
        int damage = Mathf.RoundToInt(boundUnit.Attack * damageMultiplier);
        foreach (var splashTarget in splashTargets)
        {
            if (splashTarget.IsAlive())
            {
                splashTarget.ApplyHealthChange(-damage);
                Debug.Log($"[Enemy] {gameObject.name} 对 {splashTarget.gameObject.name} 造成 {damage} 点伤害");
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return MoveTo(originalPosition, 0.3f);
    }
    
    /// <summary>
    /// 获取相邻位置
    /// </summary>
    protected List<Location> GetAdjacentLocations(Location location)
    {
        List<Location> adjacent = new List<Location>();

        switch (location)
        {
            case Location.Up:
                adjacent.Add(Location.Middle);
                break;
            case Location.Middle:
                adjacent.Add(Location.Up);
                adjacent.Add(Location.Bottom);
                break;
            case Location.Bottom:
                adjacent.Add(Location.Middle);
                break;
        }

        return adjacent;
    }

    /// <summary>
    /// 获取指定阵营的所有存活单位
    /// </summary>
    protected List<BattleUnit> GetAllUnitsOfTeam(Team team)
    {
        List<BattleUnit> units = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == team && unit.IsAlive())
                {
                    units.Add(unit);
                }
            }
        }

        return units;
    }
}

