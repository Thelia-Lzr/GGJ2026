using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskOblivionis : Mask
{
    public MaskOblivionis() : base(
        maskName: "难崩的假面",
        switchCost: 1,
        maxHealth: 11,
        atk: 3,
        atkCost: 2,
        description: "攻：溅射攻击目标及相邻敌人。当面具将被摧毁时，耐久变为1（每场战斗限1次）。")
    {

    }
    public bool usedFlag = false;
    
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
        UsageAfterAttack();
        if (controller.BoundUnit != null)
        {
            controller.BoundUnit.HealthDisplay(0);
        }
    }
    
    public override int TakeDamage(int damage)//戴着面具受到伤害
    {
        if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken && usedFlag)
        {
            OnMaskBroken();
            overflow = 0;
        }
        usedFlag = true;
        CurrentHealth = 1;
        return overflow;
    }
    // Start is called before the first frame update

}
