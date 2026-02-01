using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class MaskNeverRemoved : Mask
{
    public MaskNeverRemoved() : base(
        maskName: "从不摘下的面具",
        switchCost: 1,
        maxHealth: 8,
        atk: 5,
        atkCost: 0,
        description: "启：所有己方角色攻击力+2（持续1回合）。攻击不消耗面具耐久。",
        hasActivateAbility: true)
    {

    }
    public override IEnumerator Activate(UnitController controller)
    {
        foreach (var unit in RoundManager.Instance.battleUnits)
        {
            if (unit.UnitTeam == controller.BoundUnit.UnitTeam)
            {
                unit.ApplyStatus(new Add2AtkEffect(1));
            }
        }
        return base.Activate(controller);
    }
    // Start is called before the first frame update

}
