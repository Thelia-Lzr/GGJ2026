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

public abstract class MaskAttackPattern
{
    public string PatternName { get; protected set; }
    public string Description { get; protected set; }
    public TargetType TargetType { get; protected set; }
    
    public MaskAttackPattern(string patternName, string description, TargetType targetType)
    {
        PatternName = patternName;
        Description = description;
        TargetType = targetType;
    }
    
    public abstract void ExecuteAttack(UnitController attacker, BattleUnit target, List<BattleUnit> allTargets = null);
    
    public virtual List<BattleUnit> GetValidTargets(UnitController user, List<BattleUnit> allUnits)
    {
        List<BattleUnit> validTargets = new List<BattleUnit>();
        
        if (allUnits == null || user == null || user.BoundUnit == null)
            return validTargets;
        
        Team userTeam = user.BoundUnit.UnitTeam;
        
        switch (TargetType)
        {
            case TargetType.Single:
                foreach (var unit in allUnits)
                {
                    if (unit.IsAlive() && unit.UnitTeam != userTeam)
                        validTargets.Add(unit);
                }
                break;
            
            case TargetType.AllEnemies:
                foreach (var unit in allUnits)
                {
                    if (unit.IsAlive() && unit.UnitTeam != userTeam)
                        validTargets.Add(unit);
                }
                break;
            
            case TargetType.AllAllies:
                foreach (var unit in allUnits)
                {
                    if (unit.IsAlive() && unit.UnitTeam == userTeam)
                        validTargets.Add(unit);
                }
                break;
            
            case TargetType.Self:
                if (user.BoundUnit.IsAlive())
                    validTargets.Add(user.BoundUnit);
                break;
            
            case TargetType.Area:
                foreach (var unit in allUnits)
                {
                    if (unit.IsAlive())
                        validTargets.Add(unit);
                }
                break;
        }
        
        return validTargets;
    }
}
