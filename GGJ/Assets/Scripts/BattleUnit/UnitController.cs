using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public abstract class UnitController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] public BattleUnit boundUnit;
    [SerializeField] protected Mask currentMask;
    [SerializeField] private int attackCountGains = 1;

    protected bool isStunned = false;
    protected bool canAct = true;

    protected AnimationHandler animationHandler;

    public event Action<ActionCommand> OnActionPerformed;
    public event Action<Mask, Mask> OnMaskSwitched;
    public event Action<ActionCommand> OnActionConfirmed;

    private GameObject currentActionCircle;

    public BattleUnit BoundUnit => boundUnit;
    public Mask CurrentMask => currentMask;
    public bool CanAct => canAct && !isStunned && boundUnit != null && boundUnit.IsAlive();

    //初始属性定义
    protected int initialAttack = 10;
    protected int initialDefense = 5;
    protected int initialMaxHealth = 100;
    protected int initialHealth = 50;

    public int attackCount { get; protected set; }


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

    public void AddAttackCount(int amount)
    {
        attackCount += amount;
        Debug.Log($"[UnitController] {gameObject.name} 攻击次数增加 {amount}，当前: {attackCount}");
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
            
            // 如果是玩家回合且新面具有启效果，立即刷新黄圈（即使这回合用过）
            if (RoundManager.Instance != null && 
                RoundManager.Instance.CurrentActiveTeam == Team.Player &&
                boundUnit.UnitTeam == Team.Player &&
                currentMask.HasActivateAbility)
            {
                // 先移除旧黄圈
                boundUnit.HideActivateCircle();
                
                // 刷新启效果状态并显示新黄圈
                currentMask.CanUseActivate = true;
                boundUnit.ShowActivateCircle();
                
                Debug.Log($"[UnitController] 戴上新面具 {currentMask.MaskName}，刷新启效果黄圈");
            }
        }
        
        if (cost > 0)
        {
            SpendResource(ResourceType.ActionPoint, cost);
        }
        
        OnMaskSwitched?.Invoke(oldMask, newMask);
        
        return true;
    }
    
    public virtual void RemoveBrokenMask()
    {
        if (currentMask == null || !currentMask.IsBroken)
            return;
        
        Debug.Log($"[UnitController] 移除破损面具: {currentMask.MaskName}");
        
        Mask brokenMask = currentMask;
        brokenMask.OnUnequip(boundUnit);
        currentMask = null;
        
        if (boundUnit != null)
        {
            boundUnit.ClearMask();
        }
        
        OnMaskSwitched?.Invoke(brokenMask, null);
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
    
    public virtual IEnumerator MoveToTarget(BattleUnit target, float time)
    {
        if (target == null)
        {
            Debug.LogWarning("Invalid target for movement.");
            yield break;
        }
        
        Vector2 targetPosition = target.transform.position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float attackDistance = 1.5f;
        Vector2 attackPosition = targetPosition - direction * attackDistance;
        
        yield return MoveTo(attackPosition, time);
    }
    
    public virtual IEnumerator Attack(BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid attack target.");
            yield break;
        }
        
        Debug.Log($"{boundUnit.gameObject.name} attacks {target.gameObject.name}");
        
        Vector2 originalPosition = transform.position;
        float moveTime = 0.3f;
        
        yield return MoveToTarget(target, moveTime);

        int damage = boundUnit.Attack;
        target.ApplyHealthChange(-damage);
        
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"Return");
        yield return MoveTo(originalPosition, moveTime);
        Debug.Log($"Return/");

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
        
        // 清理现有的ActionCircle
        if (currentActionCircle != null)
        {
            Destroy(currentActionCircle);
            currentActionCircle = null;
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
    public void InitActionCircle()
    {
        //可以改为ResoruceManager来管理
        if (attackCount > 0)
        {
            if (currentActionCircle != null)
            {
                Debug.Log($"[UnitController] {gameObject.name} already has an ActionCircle, skipping creation.");
                return;
            }
            
            currentActionCircle = Instantiate(ResourceController.Instance.GetPrefab("ActionCircle"), transform);
            currentActionCircle.name = "ActionCircle"; // 确保名字正确
            ActionCircle aC = currentActionCircle.GetComponent<ActionCircle>();
            aC.Initialize(this);
            
            Debug.Log($"[UnitController] {gameObject.name} 创建ActionCircle（攻击圈）");
        }

    }
    
    public virtual void ConfirmAction(ActionCommand command)
    {
        if (command == null || !CanPerformAction(command))
        {
            Debug.LogWarning("Cannot confirm action.");
            return;
        }
        
        PerformAction(command);
        
        if (animationHandler != null)
        {
            IEnumerator actionCoroutine = GetActionCoroutine(command);
            animationHandler.SubmitAction(actionCoroutine, command, this);
        }
        else
        {
            Debug.LogWarning("AnimationHandler is not set!");
        }
        
        OnActionConfirmed?.Invoke(command);
    }
    
    protected virtual IEnumerator GetActionCoroutine(ActionCommand command)
    {
        switch (command.ActionType)
        {
            case ActionType.Attack:
                attackCount--;
                if (currentMask != null)
                {
                    yield return currentMask.Attack(this, command.Target);
                }
                else
                {
                    yield return Attack(command.Target);
                }
                break;
            
            case ActionType.SwitchMask:
                if (command.MaskData != null)
                {
                    SwitchMask(command.MaskData, command.ResourceCost);
                }
                break;
            
            case ActionType.ActivateMask:
                if (command.MaskData != null)
                {
                    Debug.Log($"[UnitController] 执行面具启效果: {command.MaskData.MaskName}");
                    yield return command.MaskData.Activate(this);
                }
                else
                {
                    Debug.LogWarning("[UnitController] ActivateMask 命令缺少 MaskData");
                }
                break;
            
            default:
                Debug.LogWarning($"Unknown action type: {command.ActionType}");
                break;
        }
    }
}
