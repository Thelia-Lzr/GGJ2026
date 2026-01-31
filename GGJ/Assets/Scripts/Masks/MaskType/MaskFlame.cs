using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskFlame : Mask
{
<<<<<<< Updated upstream
    public MaskFlame() : base(maskName: "»ðÑæÉÙÅ®µÄÃæ¾ß", switchCost: 1, maxHealth: 6, atk: 7, atkCost: 1)
    {
        
=======
    public MaskFlame() : base(maskName: "ä½ è¢«ç«ç„°åŒ…å›´äº†", switchCost: 1, maxHealth: 6, atk: 7, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/11");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask11");
>>>>>>> Stashed changes
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
