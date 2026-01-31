using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskOblivionis : Mask
{
    public MaskOblivionis() : base(maskName: "难崩的假面", switchCost: 1, maxHealth: 11, atk: 3, atkCost: 2)
    {

    }
    public bool usedFlag = false;
    
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
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
