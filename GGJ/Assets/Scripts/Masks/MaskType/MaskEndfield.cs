using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskEndfield : Mask
{
<<<<<<< Updated upstream
    public MaskEndfield() : base(maskName: "À­Ïß¹¤ÈËµÄÃæÕÖ", switchCost: 1, maxHealth: 8, atk: 0, atkCost: 1)
    {

=======
    public MaskEndfield() : base(maskName: "å¼€å§‹ç”µåŠ›è¿è¾“", switchCost: 1, maxHealth: 8, atk: 0, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/13");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask13");
>>>>>>> Stashed changes
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
