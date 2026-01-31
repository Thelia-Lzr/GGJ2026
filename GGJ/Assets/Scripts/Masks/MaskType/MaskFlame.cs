using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskFlame : Mask
{
    public MaskFlame() : base(maskName: "火焰少女的面具", switchCost: 1,maxHealth: 6,atk: 7,atkCost: 1)
    {
        
    }
    public override void OnEquip(BattleUnit unit)
    {
        base.OnEquip(unit);
    }
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        Atk += 2;
        yield return base.Attack(controller, target);
    }
    public override void OnUnequip(BattleUnit unit)
    {
        unit.addAttack(2);
    }
}
