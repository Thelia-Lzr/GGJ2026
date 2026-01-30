using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    Single,
    AllEnemies,
    AllAllies,
    Self,
    Area
}

public enum ResourceType
{
    ActionPoint,
    Mana,
    Energy,
    MaskEnergy
}

public abstract class Skill
{
    public string SkillId { get; protected set; }
    public string SkillName { get; protected set; }
    public string Description { get; protected set; }
    public TargetType TargetType { get; protected set; }
    public int Cooldown { get; protected set; }
    public int CurrentCooldown { get; protected set; }
    public ResourceType CostType { get; protected set; }
    public int Cost { get; protected set; }
    
    public Skill(string skillId, string skillName, TargetType targetType, int cooldown, ResourceType costType, int cost)
    {
        SkillId = skillId;
        SkillName = skillName;
        TargetType = targetType;
        Cooldown = cooldown;
        CurrentCooldown = 0;
        CostType = costType;
        Cost = cost;
    }
    
    public virtual bool CanUse(UnitController user)
    {
        if (CurrentCooldown > 0)
            return false;
        
        return user.HasResource(CostType, Cost);
    }
    
    public abstract void Execute(UnitController user, BattleUnit target, List<BattleUnit> additionalTargets = null);
    
    public virtual void OnCooldownTick()
    {
        if (CurrentCooldown > 0)
            CurrentCooldown--;
    }
    
    public void StartCooldown()
    {
        CurrentCooldown = Cooldown;
    }
    
    public virtual SkillSequence GetSkillSequence(UnitController user, BattleUnit target)
    {
        return new SkillSequence();
    }
}
