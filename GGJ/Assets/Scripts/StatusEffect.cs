using UnityEngine;

public abstract class StatusEffect
{
    public string StatusId { get; protected set; }
    public string StatusName { get; protected set; }
    public int Duration { get; protected set; }
    public int StackCount { get; protected set; }
    public int MaxStacks { get; protected set; }
    public bool IsStackable { get; protected set; }
    
    protected BattleUnit target;
    
    public StatusEffect(string statusId, string statusName, int duration, bool isStackable = false, int maxStacks = 1)
    {
        StatusId = statusId;
        StatusName = statusName;
        Duration = duration;
        IsStackable = isStackable;
        MaxStacks = maxStacks;
        StackCount = 1;
    }
    
    public virtual void OnApplied(BattleUnit unit)
    {
        target = unit;
    }
    
    public virtual void OnRemoved(BattleUnit unit)
    {
        target = null;
    }
    
    public virtual void OnTurnStart(BattleUnit unit)
    {
    }
    
    public virtual void OnTurnEnd(BattleUnit unit)
    {
        if (Duration > 0)
        {
            Duration--;
        }
    }
    
    public void RefreshOrStack(StatusEffect newEffect)
    {
        if (IsStackable && StackCount < MaxStacks)
        {
            StackCount++;
        }
        
        Duration = Mathf.Max(Duration, newEffect.Duration);
    }
    
    public bool ShouldRemove()
    {
        return Duration <= 0;
    }
}
