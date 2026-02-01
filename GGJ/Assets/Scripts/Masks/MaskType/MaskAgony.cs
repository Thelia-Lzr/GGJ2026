using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskAgony : Mask
{
    public MaskAgony() : base(
        maskName: "痛苦的面具",
        switchCost: 1,
        maxHealth: 6,
        atk: 3,
        atkCost: 1,
        description: "戴：眩晕1名随机敌人。攻：溅射攻击目标及相邻敌人。")
    {

    }

    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
        UsageAfterAttack();
        if (controller.BoundUnit != null)
        {
            controller.BoundUnit.HealthDisplay(0);
        }
    }

    public override void OnEquip(BattleUnit unit)
    {
        base.OnEquip(unit);
        
        Team enemyTeam = unit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetEnemyUnits(enemyTeam);
        
        if (enemies.Count > 0)
        {
            int randomIndex = Random.Range(0, enemies.Count);
            BattleUnit target = enemies[randomIndex];
            
            target.ApplyStatus(new Stunned(1));
            
            Debug.Log($"[MaskAgony] {unit.gameObject.name} 装备痛苦面具，眩晕了 {target.gameObject.name}！");
        }
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
