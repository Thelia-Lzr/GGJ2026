using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskCandleHandler : Mask
{
    public MaskCandleHandler() : base(
        maskName: "秉烛人的面具",
        switchCost: 1,
        maxHealth: 11,
        atk: 5,
        atkCost: 1,
        description: "启：若面具耐久≤4，对所有敌人造成3点群体伤害，然后回复此面具3点耐久。",
        hasActivateAbility: true)
    {
    }
    
    /// <summary>
    /// 判断当前是否可以使用启效果
    /// 条件：基础判断 + 耐久≤4
    /// </summary>
    public override bool CanUseActivateNow()
    {
        // 先检查基础条件
        if (!base.CanUseActivateNow())
            return false;
        
        // 额外条件：耐久必须≤4
        return CurrentHealth <= 4;
    }

    public override IEnumerator Activate(UnitController controller)
    {
        if (CurrentHealth <= 4)
        {
            Debug.Log($"[MaskCandleHandler] 面具耐久 ≤ 4，触发群体伤害效果！");

            Team enemyTeam = controller.BoundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
            List<BattleUnit> enemies = GetEnemyUnits(enemyTeam);

            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive())
                {
                    enemy.ApplyHealthChange(-3);
                    Debug.Log($"[MaskCandleHandler] 对 {enemy.gameObject.name} 造成 3 点伤害！");
                }
            }

            RepairMask(3);
            Debug.Log($"[MaskCandleHandler] 面具回复 3 点耐久，当前耐久: {CurrentHealth}/{MaxHealth}");
        }
        else
        {
            Debug.Log($"[MaskCandleHandler] 面具耐久 > 4 ({CurrentHealth}/{MaxHealth})，效果未触发");
        }

        yield return base.Activate(controller);
    }

    private List<BattleUnit> GetEnemyUnits(Team enemyTeam)
    {
        List<BattleUnit> enemies = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == enemyTeam && unit.IsAlive())
                {
                    enemies.Add(unit);
                }
            }
        }

        return enemies;
    }
}

