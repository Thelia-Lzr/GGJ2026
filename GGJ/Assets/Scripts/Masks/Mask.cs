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

    public int Attack => Attack;
    public int Heal => Heal; //面具的血量

    public Mask(string maskId, string maskName, int switchCost)
    {
        MaskId = maskId;
        MaskName = maskName;
        SwitchCost = switchCost;
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
}
