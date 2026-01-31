using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class MaskNeverRemoved : Mask
{
<<<<<<< Updated upstream
    public MaskNeverRemoved() : base(maskName: "�Ӳ�ժ�µ����", switchCost: 1, maxHealth: 8, atk: 5, atkCost: 0)
    {

=======
    public MaskNeverRemoved() : base(maskName: "牛战士从不摘下自己的面具！", switchCost: 1, maxHealth: 8, atk: 5, atkCost: 0)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/14");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask14");
>>>>>>> Stashed changes
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
