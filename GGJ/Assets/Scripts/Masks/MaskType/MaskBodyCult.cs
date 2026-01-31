using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskBodyCult : Mask
{
    public MaskBodyCult() : base(
        maskName: "人体派的面具",
        switchCost: 1,
        maxHealth: 8,
        atk: 3,
        atkCost: 2,
        description: "戴+启+毁：所有己方角色回复2点体力和面具耐久。攻：溅射攻击目标及相邻敌人。")
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
        HealAllAllies(unit);
    }

    public override IEnumerator Activate(UnitController controller)
    {
        if (controller != null && controller.BoundUnit != null)
        {
            HealAllAllies(controller.BoundUnit);
        }
        yield return base.Activate(controller);
    }

    public override void OnUnequip(BattleUnit unit)
    {
        HealAllAllies(unit);
        base.OnUnequip(unit);
    }

    private void HealAllAllies(BattleUnit user)
    {
        if (user == null) return;

        Team allyTeam = user.UnitTeam;
        List<BattleUnit> allies = GetAllyUnits(allyTeam);

        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyHealthChange(2);
                
                if (ally.CurrentMask != null && !ally.CurrentMask.IsBroken)
                {
                    ally.CurrentMask.RepairMask(2);
                }
            }
        }

        Debug.Log($"[MaskBodyCult] {user.gameObject.name} 的身体崇拜面具生效，所有己方角色回复2点体力和面具耐久！");
    }

    private List<BattleUnit> GetAllyUnits(Team allyTeam)
    {
        List<BattleUnit> allies = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == allyTeam && unit.IsAlive())
                {
                    allies.Add(unit);
                }
            }
        }

        return allies;
    }
}
