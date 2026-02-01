using System.Collections.Generic;

public enum ActionType
{
    Attack,
    SwitchMask,
    ActivateMask
}

public class ActionCommand
{
    public UnitController Initiator { get; set; }
    public BattleUnit Target { get; set; }
    public List<BattleUnit> Targets { get; set; }
    public ActionType ActionType { get; set; }
    public Mask MaskData { get; set; }
    public int ResourceCost { get; set; }
    
    public ActionCommand(UnitController initiator, ActionType actionType)
    {
        Initiator = initiator;
        ActionType = actionType;
        Targets = new List<BattleUnit>();
    }
    
    public ActionCommand(UnitController initiator, BattleUnit target, ActionType actionType)
    {
        Initiator = initiator;
        Target = target;
        ActionType = actionType;
        Targets = new List<BattleUnit> { target };
    }
    
    public bool IsValid()
    {
        if (Initiator == null || Initiator.BoundUnit == null || !Initiator.BoundUnit.IsAlive())
            return false;
        
        if (ActionType == ActionType.Attack)
        {
            if (Target == null || !Target.IsAlive())
                return false;
        }
        
        if (ActionType == ActionType.ActivateMask)
        {
            if (MaskData == null || !MaskData.CanUseActivateNow())
                return false;
        }
        
        return true;
    }
}
