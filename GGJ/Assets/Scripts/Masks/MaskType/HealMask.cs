using System.Collections;
using UnityEngine;

/// <summary>
/// 治疗面具 - 治疗自己而非攻击
/// </summary>
public class HealMask : Mask
{
    private int healAmount = 15;
    
    public HealMask() : base("mask_heal", "Heal Mask", 1)
    {
        Attack = 0;
        MaxHealth = 40;
        CurrentHealth = MaxHealth;
        Description = "Heals the user instead of attacking.";
    }
    
    public override IEnumerator Activate(UnitController controller, BattleUnit target)
    {
        if (controller == null)
        {
            Debug.LogWarning($"Mask {MaskName}: Invalid controller.");
            yield break;
        }
        
        if (equippedUnit != null && controller.BoundUnit != equippedUnit)
        {
            Debug.LogError($"Mask {MaskName}: Controller mismatch!");
            yield break;
        }
        
        // 治疗自己而不是攻击目标
        controller.BoundUnit.ApplyHealthChange(healAmount);
        
        Debug.Log($"{controller.BoundUnit.name} uses {MaskName} and heals for {healAmount} HP!");
        
        // 可以添加治疗动画
        yield return new WaitForSeconds(0.5f);
    }
}
