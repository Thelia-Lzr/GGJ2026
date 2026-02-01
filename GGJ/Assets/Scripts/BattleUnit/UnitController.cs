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
    
    [Header("Visual Settings")]
    [Tooltip("è§’è‰²å›¾ç‰‡Spriteï¼Œå¦‚æœä¸ºç©ºä¼šå°è¯•ä»Resources/Image/Character/ç±»ååŠ è½½")]
    [SerializeField] protected Sprite characterSprite;

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
    public Sprite CharacterSprite => characterSprite;
    
    /// <summary>
    /// è®¾ç½®è§’è‰²å›¾ç‰‡ï¼ˆç”¨äºåŠ¨æ€åˆ›å»ºæ—¶ï¼?
    /// </summary>
    public void SetCharacterSprite(Sprite sprite)
    {
        characterSprite = sprite;
    }

    //åˆå§‹å±æ€§å®šä¹?
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
        
        // å¦‚æœæ²¡æœ‰è®¾ç½®Spriteï¼Œå°è¯•ä»ResourcesåŠ è½½
        if (characterSprite == null)
        {
            string className = GetType().Name;
            characterSprite = Resources.Load<Sprite>($"Image/Char/{className}");
            if (characterSprite == null)
            {
                Debug.LogWarning($"[UnitController] æ— æ³•åŠ è½½è§’è‰²å›¾ç‰‡: Image/Char/{className}");
            }
        }
    }
    private void Start()
    {
        // åªæœ‰å½“boundUnitè¿˜æœªç»‘å®šæ—¶æ‰å°è¯•ç»‘å®šï¼ˆå…¼å®¹Inspectoré¢„è®¾ç½®çš„æƒ…å†µï¼?
        if (boundUnit != null && !isBound)
        {
            BindUnit(boundUnit);
        }
    }
    
    protected virtual void OnDestroy()
    {
        UnsubscribeFromStatusEvents();
    }
    
    protected bool isBound = false;
    
    public virtual void BindUnit(BattleUnit unit)
    {
        if (isBound && unit == boundUnit) return; // é˜²æ­¢é‡å¤ç»‘å®š
        
        boundUnit = unit;
        isBound = true;

        if (boundUnit != null)
        {
            boundUnit.Initialize(this,initialMaxHealth,initialHealth,initialAttack,initialDefense);
            SubscribeToStatusEvents();
        }
    }
    
    protected void SubscribeToStatusEvents()
    {
        if (boundUnit != null)
        {
            boundUnit.OnStatusApplied += OnStatusAppliedHandler;
            boundUnit.OnStatusRemoved += OnStatusRemovedHandler;
            boundUnit.OnDeath += OnDeathHandler;
        }
    }
    
    protected void UnsubscribeFromStatusEvents()
    {
        if (boundUnit != null)
        {
            boundUnit.OnStatusApplied -= OnStatusAppliedHandler;
            boundUnit.OnStatusRemoved -= OnStatusRemovedHandler;
            boundUnit.OnDeath -= OnDeathHandler;
        }
    }
    
    protected virtual void OnDeathHandler()
    {
        // é”€æ¯UIå…ƒç´ 
        if (boundUnit != null && boundUnit.UIText != null)
        {
            Destroy(boundUnit.UIText);
        }
        
        // é”€æ¯å•ä½GameObject
        Destroy(gameObject);
    }
    
    protected virtual void OnStatusAppliedHandler(StatusEffect effect)
    {
        // å­ç±»å¯é‡å†™æ­¤æ–¹æ³•æ¥å¤„ç†ç‰¹å®šçŠ¶æ€çš„åº”ç”¨
    }
    
    protected virtual void OnStatusRemovedHandler(StatusEffect effect)
    {
        // å­ç±»å¯é‡å†™æ­¤æ–¹æ³•æ¥å¤„ç†ç‰¹å®šçŠ¶æ€çš„ç§»é™¤
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
        Debug.Log($"[UnitController] {gameObject.name} æ”»å‡»æ¬¡æ•°å¢åŠ  {amount}ï¼Œå½“å‰? {attackCount}");
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
        
        // å°†é¢å…·ä¿¡æ¯åŒæ­¥åˆ° BattleUnit ç”¨äºæ¸²æŸ“
        if (boundUnit != null)
        {
            boundUnit.SetMask(currentMask);
            
            // å¦‚æœæ˜¯ç©å®¶å›åˆä¸”æ–°é¢å…·æœ‰å¯æ•ˆæœï¼Œç«‹å³åˆ·æ–°é»„åœˆ
            if (RoundManager.Instance != null && 
                RoundManager.Instance.CurrentActiveTeam == Team.Player &&
                boundUnit.UnitTeam == Team.Player &&
                currentMask.HasActivateAbility)
            {
                // å…ˆç§»é™¤æ—§é»„åœˆ
                boundUnit.HideActivateCircle();
                
                // åˆ·æ–°å¯æ•ˆæœçŠ¶æ€ï¼šé‡ç½®æœ¬å›åˆå¯ç”¨æ ‡è®?
                currentMask.CanUseActivateThisRound = true;
                
                // æ£€æŸ¥æ˜¯å¦æ»¡è¶³æ‰€æœ‰æ¡ä»¶åæ˜¾ç¤ºé»„åœˆ
                if (currentMask.CanUseActivateNow())
                {
                    boundUnit.ShowActivateCircle();
                    Debug.Log($"[UnitController] æˆ´ä¸Šæ–°é¢å…?{currentMask.MaskName}ï¼Œåˆ·æ–°å¯æ•ˆæœé»„åœˆ");
                }
                else
                {
                    Debug.Log($"[UnitController] æˆ´ä¸Šæ–°é¢å…?{currentMask.MaskName}ï¼Œä½†ä¸æ»¡è¶³å¯æ•ˆæœæ¡ä»¶");
                }
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
        
        Debug.Log($"[UnitController] ç§»é™¤ç ´æŸé¢å…·: {currentMask.MaskName}");
        
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
        
        // æ¸…ç†ç°æœ‰çš„ActionCircle
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

        // ¼ì²éÊÇ·ñ¿ÉÒÔĞĞ¶¯£¨ÓĞ¹¥»÷´ÎÊı»òÓĞ±©Å­×´Ì¬£©
        bool canAct = attackCount > 0 || (boundUnit != null && boundUnit.HasStatus<Enraged>());
        if (canAct)
        {
            if (currentActionCircle != null)
            {
                Debug.Log($"[UnitController] {gameObject.name} already has an ActionCircle, skipping creation.");
                return;
            }
            
            currentActionCircle = Instantiate(ResourceController.Instance.GetPrefab("ActionCircle"), transform);
            currentActionCircle.name = "ActionCircle"; // ç¡®ä¿åå­—æ­£ç¡®
            ActionCircle aC = currentActionCircle.GetComponent<ActionCircle>();
            aC.Initialize(this);
            
            Debug.Log($"[UnitController] {gameObject.name} åˆ›å»ºActionCircleï¼ˆæ”»å‡»åœˆï¼?");
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
                // ¼ì²éÊÇ·ñÓĞ±©Å­×´Ì¬
                bool hasEnraged = boundUnit != null && boundUnit.HasStatus<Enraged>();
                
                if (hasEnraged)
                {
                    // ÏûºÄ±©Å­×´Ì¬¶ø·Ç¹¥»÷´ÎÊı£¨Ö±½ÓÒÆ³ı±©Å­×´Ì¬£©
                    StatusEffect enragedEffect = boundUnit.GetStatus<Enraged>();
                    if (enragedEffect != null)
                    {
                        boundUnit.RemoveStatus(enragedEffect);
                        Debug.Log($"[UnitController] {gameObject.name} ÏûºÄ±©Å­×´Ì¬£¨ÒÑÒÆ³ı£©");
                    }
                }
                else
                {
                    // Õı³£ÏûºÄ¹¥»÷´ÎÊı
                    attackCount--;
                }
                
                if (currentMask != null)
                {
                    yield return currentMask.Attack(this, command.Target);
                }
                else
                {
                    yield return Attack(command.Target);
                }
                
                // ¹¥»÷ºó¼ì²éÊÇ·ñ»¹ÓĞ¹¥»÷´ÎÊı»ò±©Å­×´Ì¬£¬Èç¹ûÓĞÔòÖØĞÂ³õÊ¼»¯ĞĞ¶¯È¦
                bool canActAgain = attackCount > 0 || (boundUnit != null && boundUnit.HasStatus<Enraged>());
                if (canActAgain && boundUnit != null && boundUnit.IsAlive())
                {
                    // Ïú»Ùµ±Ç°ĞĞ¶¯È¦
                    if (currentActionCircle != null)
                    {
                        Destroy(currentActionCircle);
                        currentActionCircle = null;
                    }
                    // ÖØĞÂ³õÊ¼»¯ĞĞ¶¯È¦
                    InitActionCircle();
                    Debug.Log($"[UnitController] {gameObject.name} ¹¥»÷ºóÈÔ¿ÉĞĞ¶¯£¬ÖØĞÂ³õÊ¼»¯ĞĞ¶¯È¦");
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
                    Debug.Log($"[UnitController] æ‰§è¡Œé¢å…·å¯æ•ˆæ? {command.MaskData.MaskName}");
                    yield return command.MaskData.Activate(this);
                }
                else
                {
                    Debug.LogWarning("[UnitController] ActivateMask å‘½ä»¤ç¼ºå°‘ MaskData");
                }
                break;
            
            default:
                Debug.LogWarning($"Unknown action type: {command.ActionType}");
                break;
        }
    }
}
