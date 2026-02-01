using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人5（辅助debuff类）
/// Health: 20 | Atk: 5 | 攻击类型: 单体
/// 行为: 普攻*1 or 目标获得本轮攻击力-2
/// </summary>
public class EnemyDebuffer : EnemyController
{
    [Header("Debuffer Enemy Settings")]
    private int debuffAmount = 2;
    
    // 在字段初始化时设置属性
    protected new int initialHealth = 20;
    protected new int initialMaxHealth = 20;
    protected new int initialAttack = 5;
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
        List<BattleUnit> enemyUnits = GetEnemyUnits();
        
        if (enemyUnits.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 没有可攻击的敌对单位");
            return null;
        }
        
        BattleUnit target = SelectRandomTarget(enemyUnits);
        ActionCommand action = new ActionCommand(this, target, ActionType.Attack);
        
        return action;
    }
    
    protected override IEnumerator GetActionCoroutine(ActionCommand command)
    {
        if (command.ActionType == ActionType.Attack)
        {
            attackCount--;
            
            // 随机选择：普通攻击 or 给目标减攻击力
            if (Random.value > 0.5f && command.Target != null)
            {
                // 给目标减攻击力
                Debug.Log($"[EnemyDebuffer] {gameObject.name} 给 {command.Target.gameObject.name} 降低攻击力 (-{debuffAmount})");
                yield return DebuffTarget(command.Target);
            }
            else
            {
                // 普通单体攻击
                yield return AttackSingle(command.Target, 1f);
            }
        }
        else
        {
            yield return base.GetActionCoroutine(command);
        }
    }
    
    private IEnumerator DebuffTarget(BattleUnit target)
    {
        if (target != null && target.IsAlive())
        {
            target.ApplyStatus(new Minus2AtkEffect(1));
            Debug.Log($"[EnemyDebuffer] {target.gameObject.name} 攻击力降低 {debuffAmount}");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private List<BattleUnit> GetEnemyUnits()
    {
        List<BattleUnit> enemyUnits = new List<BattleUnit>();
        Team enemyTeam = boundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        
        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == enemyTeam && unit.IsAlive())
                {
                    enemyUnits.Add(unit);
                }
            }
        }
        
        return enemyUnits;
    }
    
    private BattleUnit SelectRandomTarget(List<BattleUnit> targets)
    {
        int randomIndex = Random.Range(0, targets.Count);
        return targets[randomIndex];
    }
}
