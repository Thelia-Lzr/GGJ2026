using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskOblivionis : Mask
{
    public MaskOblivionis() : base(maskName: "蓝发少女的面具", switchCost: 1, maxHealth: 11, atk: 3, atkCost: 1)
    {

    }
    public bool usedFlag = false;
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
