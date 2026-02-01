using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（辅助buff类）兼肉盾 3
/// Health: 25 | Atk: 3 | 攻击类型: 单体
/// 行为: 普攻*1 and 全体队友本回合攻击力+3（同时执行）
/// </summary>
public class EnemyBuffer3 : EnemyController
{
    [Header("Buffer Enemy Settings")]
    private int buffAmount = 3;
    
    protected new int initialHealth = 25;
    protected new int initialMaxHealth = 25;
    protected new int initialAttack = 3;
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
            
            // 普攻*1 AND 全体队友本回合攻击力+3（同时执行）
            // 先攻击
            yield return AttackSingle(command.Target, 1f);
            
            // 再给所有队友加攻击力BUFF
            Debug.Log($"[EnemyBuffer3] {gameObject.name} 给所有队友加攻击力BUFF (+{buffAmount})");
            yield return BuffAllies();
        }
        else
        {
            yield return base.GetActionCoroutine(command);
        }
    }
    
    private IEnumerator BuffAllies()
    {
        List<BattleUnit> allies = GetAllyUnits();
        
        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyStatus(new Add3AtkEffect(1));
                Debug.Log($"[EnemyBuffer3] {ally.gameObject.name} 获得攻击力+{buffAmount} BUFF");
            }
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
