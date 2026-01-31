using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（辅助回血类）
/// Health: 15 | Atk: 5 | 攻击类型: 单体
/// 行为: 单体1Atk or 全体队友回复2点体力
/// </summary>
public class EnemyHealer : EnemyController
{
    [Header("Healer Enemy Settings")]
    private int initialHealth = 15;
    private int initialAttack = 5;
    private int healAmount = 2;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
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
            
            // 随机选择：普通攻击 or 给队友回血
            if (Random.value > 0.5f)
            {
                // 给所有队友回血
                Debug.Log($"[EnemyHealer] {gameObject.name} 给所有队友回复体力 (+{healAmount})");
                yield return HealAllies();
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
    
    private IEnumerator HealAllies()
    {
        List<BattleUnit> allies = GetAllyUnits();
        
        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyHealthChange(healAmount);
                Debug.Log($"[EnemyHealer] {ally.gameObject.name} 回复了 {healAmount} 点体力");
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
