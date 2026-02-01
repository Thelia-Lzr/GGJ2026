using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskEndfield : Mask
{
    public MaskEndfield() : base(
        maskName: "拉线工人的面罩",
        switchCost: 1,
        maxHealth: 8,
        atk: 0,
        atkCost: 1,
        description: "戴+启：抽1张卡。攻：群体攻击所有敌人。",
        hasActivateAbility: true)
    {

    }
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackAOE(controller, target);
        UsageAfterAttack();
        if (controller.BoundUnit != null)
        {
            controller.BoundUnit.HealthDisplay(0);
        }
    }
    public override void OnEquip(BattleUnit unit)
    {
        HandManager.Instance.DrawCard();
        base.OnEquip(unit);
    }
    public override IEnumerator Activate(UnitController controller)
    {
        HandManager.Instance.DrawCard();
        yield return base.Activate(controller);
    }
}
