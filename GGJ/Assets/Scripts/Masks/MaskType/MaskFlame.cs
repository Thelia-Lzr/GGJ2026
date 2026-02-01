using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskFlame : Mask
{
    public MaskFlame() : base(
        maskName: "火焰少女的面具",
        switchCost: 1,
        maxHealth: 6,
        atk: 7,
        atkCost: 1,
        description: "攻：攻击力额外+2。毁：己方单位攻击力+2。")
    {
        
    }
    public override void OnEquip(BattleUnit unit)
    {
        base.OnEquip(unit);
    }
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        Atk += 2;
        yield return AttackSingle(controller, target);
        UsageAfterAttack();
        if (controller.BoundUnit != null)
        {
            controller.BoundUnit.HealthDisplay(0);
        }
    }
    public override void OnUnequip(BattleUnit unit)
    {
        unit.addAttack(2);
    }
}
