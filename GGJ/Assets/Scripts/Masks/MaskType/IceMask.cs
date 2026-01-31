using System.Collections;
using UnityEngine;

/// <summary>
/// 冰霜面具 - 造成冰霜伤害并减速敌人
/// </summary>
public class IceMask : Mask
{
    public IceMask() : base("mask_ice", "Ice Mask", 1)
    {
        Attack = 3;
        MaxHealth = 60;
        CurrentHealth = MaxHealth;
        Description = "Deals ice damage and may slow the target.";
    }
    
    public override IEnumerator Activate(UnitController controller, BattleUnit target)
    {
        if (controller == null || target == null)
        {
            Debug.LogWarning($"Mask {MaskName}: Invalid activation parameters.");
            yield break;
        }
        
        if (equippedUnit != null && controller.BoundUnit != equippedUnit)
        {
            Debug.LogError($"Mask {MaskName}: Controller mismatch!");
            yield break;
        }
        
        Vector2 originalPos = controller.transform.position;
        Vector2 targetPos = target.transform.position;
        Vector2 direction = (targetPos - originalPos).normalized;
        Vector2 attackPos = targetPos - direction * 1.5f;
        
        yield return controller.MoveTo(attackPos, 0.3f);
        
        int damage = controller.BoundUnit.Attack + this.Attack;
        target.ApplyHealthChange(-damage);
        
        Debug.Log($"{controller.BoundUnit.name} attacks with {MaskName}, dealing {damage} ice damage!");
        
        // TODO: 添加减速状态效果
        // target.ApplyStatus(new SlowStatus(2));
        
        yield return new WaitForSeconds(0.1f);
        yield return controller.MoveTo(originalPos, 0.3f);
    }
}
