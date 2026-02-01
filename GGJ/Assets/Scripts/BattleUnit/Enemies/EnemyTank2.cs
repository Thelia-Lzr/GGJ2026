using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（攻击单人类）兼肉盾 2
/// Health: 30 | Atk: 7 | 攻击类型: 单体
/// 行为: 普攻*1 or 蓄力1回合 普攻*2.5（向上取整）
/// </summary>
public class EnemyTank2 : EnemyController
{
    [Header("Tank Enemy Settings")]
    [SerializeField] private int chargeCounter = 0;
    [SerializeField] private int chargeTime = 1;
    [SerializeField] private float chargeMultiplier = 2.5f;
    
    protected new int initialHealth = 30;
    protected new int initialMaxHealth = 30;
    protected new int initialAttack = 7;
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
    
    protected override void OnStatusAppliedHandler(StatusEffect effect)
    {
        base.OnStatusAppliedHandler(effect);
        
        if (effect is Stunned)
        {
            Debug.Log($"[EnemyTank2] {gameObject.name} 被眩晕！蓄力计数器重置。");
            chargeCounter = 0;
        }
    }
    
    protected override void OnStatusRemovedHandler(StatusEffect effect)
    {
        base.OnStatusRemovedHandler(effect);
        
        if (effect is Stunned)
        {
            Debug.Log($"[EnemyTank2] {gameObject.name} 眩晕解除。");
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
            
            // 如果已经蓄力满了，必须释放蓄力攻击（向上取整）
            if (chargeCounter >= chargeTime)
            {
                Debug.Log($"[EnemyTank2] {gameObject.name} 释放蓄力攻击！");
                yield return AttackSingleRoundUp(command.Target, chargeMultiplier);
                chargeCounter = 0;
            }
            // 如果正在蓄力中（但未满），继续蓄力
            else if (chargeCounter > 0 && chargeCounter < chargeTime)
            {
                chargeCounter++;
                Debug.Log($"[EnemyTank2] {gameObject.name} 继续蓄力 ({chargeCounter}/{chargeTime})");
                yield return new WaitForSeconds(0.5f);
            }
            // 如果未在蓄力状态，随机选择普通攻击或开始蓄力
            else
            {
                if (Random.value > 0.5f)
                {
                    // 开始蓄力
                    chargeCounter++;
                    Debug.Log($"[EnemyTank2] {gameObject.name} 开始蓄力 ({chargeCounter}/{chargeTime})");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    // 普通攻击
                    yield return AttackSingle(command.Target);
                }
            }
        }
        else
        {
            yield return base.GetActionCoroutine(command);
        }
    }
    
    private IEnumerator AttackSingleRoundUp(BattleUnit target, float multiplier)
    {
        if (target != null && target.IsAlive())
        {
            int damage = Mathf.CeilToInt(boundUnit.Attack * multiplier);
            target.ApplyHealthChange(-damage);
            Debug.Log($"[EnemyTank2] {gameObject.name} 对 {target.gameObject.name} 造成 {damage} 点蓄力伤害");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    public override void OnTurnStart()
    {
        base.OnTurnStart();
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
