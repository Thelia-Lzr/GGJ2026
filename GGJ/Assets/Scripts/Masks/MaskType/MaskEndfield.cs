using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskEndfield : Mask
{
    public MaskEndfield() : base(maskName: "开始电力运输！", switchCost: 1, maxHealth: 8, atk: 0, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/Card3");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask3");
    }
    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackAOE(controller, target);
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
