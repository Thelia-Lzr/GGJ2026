using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（辅助回血类）3
/// Health: 30 | Atk: 0 | 攻击类型: 单体
/// 行为: 全体队友回复3点体力（不攻击）
/// </summary>
public class EnemyHealer3 : EnemyController
{
    [Header("Healer Enemy Settings")]
    private int healAmount = 3;
    
    protected new int initialHealth = 30;
    protected new int initialMaxHealth = 30;
    protected new int initialAttack = 0;
    protected new int initialDefense = 5;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void BindUnit(BattleUnit unit)
    {
        if (isBound && unit == boundUnit) return;
        
        boundUnit = unit;
        isBound = true;
        
        if (boundUnit != null)
        {
            boundUnit.Initialize(this, initialMaxHealth, initialHealth, initialAttack, initialDefense);
            SubscribeToStatusEvents();
        }
    }
    
    public override ActionCommand AI()
    {
        List<BattleUnit> allyUnits = GetAllyUnits();
        
        if (allyUnits.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 没有可治疗的队友");
            return null;
        }
        
        // 选择一个队友作为目标（虽然实际会治疗全体）
        BattleUnit target = SelectRandomTarget(allyUnits);
        ActionCommand action = new ActionCommand(this, target, ActionType.Attack);
        
        return action;
    }
    
    protected override IEnumerator GetActionCoroutine(ActionCommand command)
    {
        if (command.ActionType == ActionType.Attack)
        {
            attackCount--;
            
            // 只回血，不攻击
            Debug.Log($"[EnemyHealer3] {gameObject.name} 给所有队友恢复体力 (+{healAmount})");
            yield return HealAllies();
        }
        else
        {
            yield return base.GetActionCoroutine(command);
        }
    }
    
    private IEnumerator HealAllies()
    {
        List<BattleUnit> allies = GetAllyUnits();
        
        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyHealthChange(healAmount);
                Debug.Log($"[EnemyHealer3] {ally.gameObject.name} 恢复了 {healAmount} 点体力");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private List<BattleUnit> GetAllyUnits()
    {
        List<BattleUnit> allyUnits = new List<BattleUnit>();
        Team allyTeam = boundUnit.UnitTeam;
        
        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == allyTeam && unit.IsAlive())
                {
                    allyUnits.Add(unit);
                }
            }
        }
        
        return allyUnits;
    }
    
    private BattleUnit SelectRandomTarget(List<BattleUnit> targets)
    {
        int randomIndex = Random.Range(0, targets.Count);
        return targets[randomIndex];
    }
}
