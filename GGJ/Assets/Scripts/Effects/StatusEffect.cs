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
public class Add2AtkEffect : StatusEffect
{
    public Add2AtkEffect(int duration) : base(statusId: "Add2Atk", statusName: "增加2点攻击力", duration: duration)
    {
    }
    public override void OnApplied(BattleUnit unit)
    {
        unit.addAttack(2);
        base.OnApplied(unit);
    }
    public override void OnRemoved(BattleUnit unit)
    {
        unit.addAttack(-2);
        base.OnRemoved(unit);
    }
}

public class Add3AtkEffect : StatusEffect
{
    public Add3AtkEffect(int duration) : base(statusId: "Add3Atk", statusName: "增加3点攻击力", duration: duration)
    {
    }
    public override void OnApplied(BattleUnit unit)
    {
        unit.addAttack(3);
        base.OnApplied(unit);
    }
    public override void OnRemoved(BattleUnit unit)
    {
        unit.addAttack(-3);
        base.OnRemoved(unit);
    }
}

public class Minus2AtkEffect : StatusEffect
{
    public Minus2AtkEffect(int duration) : base(statusId: "Minus2Atk", statusName: "减少2点攻击力", duration: duration)
    {
    }
    public override void OnApplied(BattleUnit unit)
    {
        unit.addAttack(-2);
        base.OnApplied(unit);
    }
    public override void OnRemoved(BattleUnit unit)
    {
        unit.addAttack(2);
        base.OnRemoved(unit);
    }
}
public class Stunned: StatusEffect
{
    public Stunned(int duration) : base(statusId: "Stunned", statusName: "眩晕", duration: duration)
    {
    }
}

public class Enraged : StatusEffect
{
    public Enraged(int duration) : base(statusId: "Enraged", statusName: "暴怒", duration: duration)
    {
    }
    
    public override void OnApplied(BattleUnit unit)
    {
        base.OnApplied(unit);
        
        // 如果角色没有攻击次数，增加一次攻击次数并移除暴怒状态
        if (unit.Controller != null && unit.Controller.attackCount <= 0)
        {
            unit.Controller.AddAttackCount(1);
            
            // 只有玩家单位需要初始化行动圈
            if (unit.UnitTeam == Team.Player)
            {
                unit.Controller.InitActionCircle();
            }
            
            // 增加攻击次数后立即移除暴怒状态
            unit.RemoveStatus(this);
            Debug.Log($"[Enraged] {unit.gameObject.name} 获得暴怒状态，增加攻击次数并移除暴怒状态");
        }
    }
    
    // 暴怒效果：如果有攻击次数时获得，攻击时消耗暴怒状态而非攻击次数
    // 在UnitController.GetActionCoroutine中处理消耗逻辑
}