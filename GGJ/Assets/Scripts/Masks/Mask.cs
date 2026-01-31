using System.Collections.Generic;
using UnityEngine;


public abstract class Mask
{
    public string MaskId { get; protected set; }
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }
    
    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Attack { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }
    
    public bool IsBroken => CurrentHealth <= 0;

    public Mask(string maskId, string maskName, int switchCost)
    {
        MaskId = maskId;
        MaskName = maskName;
        SwitchCost = switchCost;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Attack = 0;
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
    
    public virtual void Activate(PlayerController controller)
    {
    }
    
    public MaskAttackPattern GetAttackPattern()
    {
        return attackPattern;
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
    
    public virtual int TakeDamage(int damage)
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
        Debug.Log($"Ãæ¾ß {MaskName} ÒÑÆÆËé£¡");
    }
}
