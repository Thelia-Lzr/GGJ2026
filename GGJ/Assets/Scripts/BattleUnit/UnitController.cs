using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class UnitController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] protected BattleUnit boundUnit;
    [SerializeField] protected Mask currentMask;
    [SerializeField] private int attackCountGains = 1;
    
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

    protected int attackCount = 0;


    protected virtual void Awake()
    {
        animationHandler = AnimationHandler.Instance;
        
        if (animationHandler == null)
        {
            Debug.LogWarning($"UnitController on {gameObject.name} could not find AnimationHandler in scene!");
        }
    }
    private void Start()
    {
        BindUnit(boundUnit);
    }
    public virtual void BindUnit(BattleUnit unit)
    {
        boundUnit = unit;

        if (boundUnit != null)
        {
            boundUnit.Initialize(this,initialMaxHealth,initialHealth,initialAttack,initialDefense);
        }
    }
    
    
    public abstract void TakeTurn();
    
    public abstract bool CanPerformAction(ActionCommand command);
    
    public abstract void PerformAction(ActionCommand command);
    
    public abstract bool HasResource(ResourceType type, int amount);
    
    public abstract void SpendResource(ResourceType type, int amount);
    
    public abstract void GainResource(ResourceType type, int amount);
    
    public abstract int GetResource(ResourceType type);
    
    protected void RaiseActionPerformed(ActionCommand command)
    {
        OnActionPerformed?.Invoke(command);
    }
    
    public void GetAttackCount()
    {
        attackCount = attackCountGains;
    }
    public virtual bool SwitchMask(Mask newMask, int cost)
    {
        if (newMask == null)
            return false;
        
        if (cost > 0 && !HasResource(ResourceType.ActionPoint, cost))
            return false;
        
        Mask oldMask = currentMask;
        
        if (oldMask != null)
        {
            oldMask.OnUnequip(boundUnit);
        }
        
        currentMask = newMask;
        currentMask.OnEquip(boundUnit);
        
        // 将面具信息同步到 BattleUnit 用于渲染
        if (boundUnit != null)
        {
            boundUnit.SetMask(currentMask);
        }
        
        if (cost > 0)
        {
            SpendResource(ResourceType.ActionPoint, cost);
        }
        
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
    public IEnumerator MoveTo(Vector2 targetPosition,float time)
    {
        var movePosition =((Vector3)targetPosition-transform.position)/time;
        while (time>0)
        {
            time-=Time.deltaTime;
            transform.position += movePosition*Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}
