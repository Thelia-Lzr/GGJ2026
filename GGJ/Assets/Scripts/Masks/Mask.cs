using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面具基类
/// 
/// 在实现 Activate 方法时的重要规范：
/// - 始终使用传入的 controller 参数进行移动、攻击等操作
/// - 不要直接使用 equippedUnit.transform，而应该使用 controller.transform
/// - controller.BoundUnit 应该与 equippedUnit 一致
/// - 所有协程操作（MoveTo, Attack等）都应该由 controller 发起
/// </summary>
public abstract class Mask
{
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }

    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Atk { get; protected set; }

    public int AtkCost { get; protected set; } //每次使用该面具进行攻击消耗的耐久
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }

    public bool IsBroken => CurrentHealth <= 0;

    public Mask(string maskName, int switchCost,int maxHealth,int atk,int atkCost)
    {
        MaskName = maskName;
        SwitchCost = switchCost;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Atk = atk;
        AtkCost = atkCost;
    }

    public virtual void OnAddedToInventory()
    {
    }

    public virtual void OnRemovedFromInventory()
    {
    }

    public virtual bool CanUseInBattle()
    {
        return true;
    }

    public virtual void OnEquip(BattleUnit unit)
    {
        equippedUnit = unit;
    }

    public virtual void OnUnequip(BattleUnit unit)
    {
        equippedUnit = null;
    }

    public virtual IEnumerator Activate(UnitController controller, BattleUnit target)//实现面具启效果
    {
        this.UsageAfterAttack();

        // 默认行为：使用传入的 controller 执行基础攻击
        yield return controller.Attack(target);
    }


    public int GetSwitchCost()
    {
        return SwitchCost;
    }

    public virtual void OnTurnStart()
    {
    }

    public virtual void OnTurnEnd()
    {
    }

    public virtual void UsageAfterAttack()//面具使用后耐久减少
    {
        int damage = AtkCost;
        //if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return;
    }
    public virtual int TakeDamage(int damage)//戴着面具受到伤害
    {
        if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return overflow;
    }

    public virtual void RepairMask(int amount)
    {
        if (IsBroken) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }

    public virtual void FullyRepair()
    {
        CurrentHealth = MaxHealth;
    }

    protected virtual void OnMaskBroken()
    {
        Debug.Log($"面具 {MaskName} 已破碎！");
    }
}