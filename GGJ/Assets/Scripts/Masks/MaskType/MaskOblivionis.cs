using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskOblivionis : Mask
{
<<<<<<< Updated upstream
    public MaskOblivionis() : base(maskName: "难崩的假面", switchCost: 1, maxHealth: 11, atk: 3, atkCost: 2)
    {

=======

    public MaskOblivionis() : base(maskName: "姣嶉浮鍗＄殑涓栫晫瑙傦紒", switchCost: 1, maxHealth: 11, atk: 3, atkCost: 2)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/12");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask12");
>>>>>>> Stashed changes
    }
    public bool usedFlag = false;
    
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
    }
    
<<<<<<< Updated upstream
    public override int TakeDamage(int damage)//戴着面具受到伤害
=======
    public override int TakeDamage(int damage)//鎴寸潃闈㈠叿鍙楀埌浼ゅ
>>>>>>> Stashed changes
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
