using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class UnitController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] protected BattleUnit boundUnit;
    [SerializeField] protected Mask currentMask;
    
    protected Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    protected bool isStunned = false;
    protected bool canAct = true;
    
    protected AnimationHandler animationHandler;
    
    public event Action<ActionCommand> OnActionPerformed;
    public event Action<Mask, Mask> OnMaskSwitched;
    
    public BattleUnit BoundUnit => boundUnit;
    public Mask CurrentMask => currentMask;
    public bool CanAct => canAct && !isStunned && boundUnit != null && boundUnit.IsAlive();

    //初始属性定义
    private int initialAttack = 10;
    private int initialDefense = 5;
    private int initialMaxHealth = 100;
    private int initialHealth = 50;

    protected virtual void Awake()
    {
        InitializeResources();
        
        animationHandler = AnimationHandler.Instance;
        
        if (animationHandler == null)
        {
            Debug.LogWarning($"UnitController on {gameObject.name} could not find AnimationHandler in scene!");
        }
    }
    
    protected virtual void InitializeResources()
    {
        resources[ResourceType.Mana] = 0;
        resources[ResourceType.Energy] = 0;
        resources[ResourceType.MaskEnergy] = 100;
    }
    
    public virtual void BindUnit(BattleUnit unit)
    {
        boundUnit = unit;
        boundUnit.Attack = initialAttack;
        

        if (boundUnit != null)
        {
            boundUnit.Initialize(this);
        }
    }
    
    public void SetAnimationHandler(AnimationHandler handler)
    {
        if (handler != null)
        {
            animationHandler = handler;
        }
        else
        {
            Debug.LogWarning("Attempted to set null AnimationHandler!");
        }
    }
    
    public abstract void TakeTurn();
    
    public virtual bool CanPerformAction(ActionCommand command)
    {
        if (!CanAct)
            return false;
        
        if (command == null || !command.IsValid())
            return false;
        
        return HasResource(ResourceType.ActionPoint, command.ResourceCost);
    }
    
    public virtual void PerformAction(ActionCommand command)
    {
        if (!CanPerformAction(command))
        {
            Debug.LogWarning($"Cannot perform action: {command.ActionType}");
            return;
        }
        
        SpendResource(ResourceType.ActionPoint, command.ResourceCost);
        
        OnActionPerformed?.Invoke(command);
    }
    
    public bool HasResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            return false;
        
        return resources[type] >= amount;
    }
    
    public void SpendResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
        {
            resources[type] = 0;
        }
        
        resources[type] = Mathf.Max(0, resources[type] - amount);
    }
    
    public void GainResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
        {
            resources[type] = 0;
        }
        
        resources[type] += amount;
    }
    
    public int GetResource(ResourceType type)
    {
        if (!resources.ContainsKey(type))
            return 0;
        
        return resources[type];
    }
    
    public virtual bool SwitchMask(Mask newMask, int cost)
    {
        if (newMask == null)
            return false;
        
        if (!HasResource(ResourceType.MaskEnergy, cost))
            return false;
        
        Mask oldMask = currentMask;
        
        if (oldMask != null)
        {
            oldMask.OnUnequip(boundUnit);
        }
        
        currentMask = newMask;
        currentMask.OnEquip(boundUnit);
        
        SpendResource(ResourceType.MaskEnergy, cost);
        
        OnMaskSwitched?.Invoke(oldMask, newMask);
        
        return true;
    }
    
    public List<ActionCommand> GetAvailableActions()
    {
        List<ActionCommand> actions = new List<ActionCommand>();
        
        if (!CanAct)
            return actions;
        
        ActionCommand attackAction = new ActionCommand(this, ActionType.Attack)
        {
            ResourceCost = 1
        };
        actions.Add(attackAction);
        
        return actions;
    }
    
    public virtual IEnumerator Attack(BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid attack target.");
            yield break;
        }
        
        Debug.Log($"{boundUnit.gameObject.name} attacks {target.gameObject.name}");
        
        
        yield return null;
    }
    
    public virtual void OnTurnStart()
    {
        if (boundUnit != null)
        {
            boundUnit.OnTurnStart();
        }
        
        if (currentMask != null)
        {
            currentMask.OnTurnStart();
        }
    }
    
    public virtual void OnTurnEnd()
    {
        if (boundUnit != null)
        {
            boundUnit.OnTurnEnd();
        }
        
        if (currentMask != null)
        {
            currentMask.OnTurnEnd();
        }
    }
    
    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }
    
    public void SetCanAct(bool can)
    {
        canAct = can;
    }
}
