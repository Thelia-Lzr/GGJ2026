using System.Collections;
using UnityEngine;

/// <summary>
/// 火焰面具 - 造成额外火焰伤害并点燃敌人
/// </summary>
public class FireMask : Mask
{
    public FireMask() : base("mask_fire", "Fire Mask", 1)
    {
        Attack = 5;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Description = "Deals fire damage and may burn the target.";
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
        
        // 保存原始位置
        Vector2 originalPos = controller.transform.position;
        Vector2 targetPos = target.transform.position;
        
        // 计算攻击位置
        Vector2 direction = (targetPos - originalPos).normalized;
        Vector2 attackPos = targetPos - direction * 1.5f;
        
        // 移动到目标
        yield return controller.MoveTo(attackPos, 0.3f);
        
        // 造成火焰伤害 (基础攻击 + 面具攻击)
        int damage = controller.BoundUnit.Attack + this.Attack;
        target.ApplyHealthChange(-damage);
        
        Debug.Log($"{controller.BoundUnit.name} attacks with {MaskName}, dealing {damage} fire damage!");
        
        // TODO: 添加燃烧状态效果
        // target.ApplyStatus(new BurnStatus(3, 2));
        
        // 等待一下
        yield return new WaitForSeconds(0.1f);
        
        // 返回原位
        yield return controller.MoveTo(originalPos, 0.3f);
    }
}
