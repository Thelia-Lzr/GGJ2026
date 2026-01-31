using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskAgony : Mask
{
    public MaskAgony() : base(maskName: "折磨开始", switchCost: 1, maxHealth: 6, atk: 3, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/Card5");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask5");
    }

    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
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
