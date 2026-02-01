using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人（攻击单人类）兼肉盾
/// Health: 30 | Atk: 5 | 攻击类型: 单体
/// 行为: 单体Atk or 蓄力2回合 单体2.6Atk
/// </summary>
public class EnemyTank : EnemyController
{
    [Header("Tank Enemy Settings")]
    [SerializeField] private int chargeCounter = 0;
    [SerializeField] private int chargeTime = 1;
    [SerializeField] private float chargeMultiplier = 2.6f;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        initialHealth = 30;
        initialAttack = 5;
        
        if (boundUnit != null)
        {
            boundUnit.Initialize(this, initialHealth, initialHealth, initialAttack, 5);
        }
    }
    
    protected override void OnStatusAppliedHandler(StatusEffect effect)
    {
        base.OnStatusAppliedHandler(effect);
        
        if (effect is Stunned)
        {
            Debug.Log($"[EnemyTank] {gameObject.name} 被眩晕！蓄力计数器重置。");
            chargeCounter = 0;
        }
    }
    
    protected override void OnStatusRemovedHandler(StatusEffect effect)
    {
        base.OnStatusRemovedHandler(effect);
        
        if (effect is Stunned)
        {
            Debug.Log($"[EnemyTank] {gameObject.name} 眩晕解除。");
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
            
            // 如果已经蓄力满了，必须释放蓄力攻击
            if (chargeCounter >= chargeTime)
            {
                Debug.Log($"[EnemyTank] {gameObject.name} 释放蓄力攻击！");
                yield return AttackSingle(command.Target, chargeMultiplier);
                chargeCounter = 0;
            }
            // 如果正在蓄力中（但未满），继续蓄力
            else if (chargeCounter > 0 && chargeCounter < chargeTime)
            {
                chargeCounter++;
                Debug.Log($"[EnemyTank] {gameObject.name} 继续蓄力 ({chargeCounter}/{chargeTime})");
                yield return new WaitForSeconds(0.5f);
            }
            // 如果未在蓄力状态，随机选择普通攻击或开始蓄力
            else
            {
                if (Random.value > 0.5f)
                {
                    // 开始蓄力
                    chargeCounter++;
                    Debug.Log($"[EnemyTank] {gameObject.name} 开始蓄力 ({chargeCounter}/{chargeTime})");
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
    public bool JudgeCharge()
    {
        if (chargeTime !=0)
        {
            return true;
        }
        return false;
    }
}
