using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（攻击溅射类）
/// Health: 25 | Atk: 3 | 攻击类型: 溅射
/// 行为: 溅射1Atk
/// </summary>
public class EnemySplash : EnemyController
{
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        initialHealth = 25;
        initialAttack = 3;
        
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
            yield return AttackSplash(command.Target, 1f);
        }
        else
        {
            yield return base.GetActionCoroutine(command);
        }
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
