using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（辅助debuff类）
/// Health: 20 | Atk: 5 | 攻击类型: 单体
/// 行为: 单体1Atk or 目标本回合攻击力-2
/// </summary>
public class EnemyDebuffer : EnemyController
{
    [Header("Debuffer Enemy Settings")]
    private int debuffAmount = 2;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        initialHealth = 20;
        initialAttack = 5;
        
        if (boundUnit != null)
        {
            boundUnit.Initialize(this, initialHealth, initialHealth, initialAttack, 5);
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
