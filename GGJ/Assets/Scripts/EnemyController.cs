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
    
    protected override void Awake()
    {
        base.Awake();
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
        }
        
        OnTurnEnd();
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
        
        List<ActionCommand> usableSkills = availableActions.Where(a => 
            a.ActionType == ActionType.Skill && 
            a.SkillData != null && 
            a.SkillData.CanUse(this)
        ).ToList();
        
        if (usableSkills.Count > 0 && ShouldUseSkill())
        {
            ActionCommand skillCommand = usableSkills[Random.Range(0, usableSkills.Count)];
            skillCommand.Target = target;
            return skillCommand;
        }
        
        ActionCommand attackCommand = new ActionCommand(this, target, ActionType.Attack)
        {
            ResourceCost = 1
        };
        
        return attackCommand;
    }
    
    protected virtual bool ShouldUseSkill()
    {
        switch (behaviorType)
        {
            case AIBehaviorType.Aggressive:
                return Random.value > 0.3f;
            
            case AIBehaviorType.Defensive:
                return boundUnit.CurrentHealth < boundUnit.MaxHealth * 0.5f;
            
            case AIBehaviorType.Balanced:
                return Random.value > 0.5f;
            
            case AIBehaviorType.Random:
                return Random.value > 0.5f;
            
            default:
                return Random.value > 0.5f;
        }
    }
    
    public void SetBehaviorType(AIBehaviorType type)
    {
        behaviorType = type;
    }
}
