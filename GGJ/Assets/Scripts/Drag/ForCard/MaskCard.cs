using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MaskCard : DragUnit
{
    [Header("Card Display References")]
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro descriptionText;
    [SerializeField] private SpriteRenderer iconSprite;
    
    [Header("Card Data")]
    [SerializeField] private Mask mask;
    
    [Header("Interaction Settings")]
    [SerializeField] private float validInteractionDistance = 2.0f;
    
    private RoundManager roundManager => RoundManager.Instance;
    private HandManager handManager => HandManager.Instance;
    private PlayerResourceManager playerResourceManager => PlayerResourceManager.Instance;
    private BattleUnit targetUnit;
    
    // MaskCard 特有的状态
    private bool isReturning = false;
    private ZoomUI zoomUI;
    
    public Mask Mask 
    { 
        get => mask; 
        set 
        {
            mask = value;
            UpdateCardDisplay();
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        zoomUI = GetComponent<ZoomUI>();
    }
    
    void Start()
    {
        if (mask != null)
        {
            UpdateCardDisplay();
        }
    }
    
    private void UpdateCardDisplay()
    {
        if (mask == null) return;
        
        if (nameText != null)
        {
            nameText.text = mask.MaskName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = mask.Description;
        }
        
        if (iconSprite != null && mask.MaskIcon != null)
        {
            iconSprite.sprite = mask.MaskIcon;
        }
    }
    
    // 重写 OnMouseDown 来添加 MaskCard 特有的行为
    protected override void OnMouseDown()
    {
        if (isReturning) return; // MaskCard 特有：返回中不允许拖动
        
        // 通知 ZoomUI 开始拖动
        if (zoomUI != null)
        {
            zoomUI.OnDragStart();
        }
        
        // 调用基类的行为
        base.OnMouseDown();
    }
    
    // 重写 OnMouseUp 来添加 MaskCard 特有的行为
    protected override void OnMouseUp()
    {
        // 调用基类的行为
        base.OnMouseUp();
        
        // 通知 ZoomUI 拖动结束
        if (zoomUI != null)
        {
            zoomUI.OnDragEnd();
        }
    }
    
    // 重写 ReturnBackAction 来使用 HandManager 的动画系统
    protected override IEnumerator ReturnBackAction()
    {
        isReturning = true;
        isDragging = false;
        dragController.Status = 0;
        
        // 立即通知 HandManager 重新整理手牌位置
        if (handManager != null)
        {
            handManager.RefreshCardPositions();
        }
        
        // 等待 HandManager 的动画完成
        yield return new WaitForSeconds(0.25f);
        
        isReturning = false;
    }
    
    protected override bool isMatch()
    {
        if (roundManager == null || roundManager.GetActiveTeam() != Team.Player)
        {
            Debug.Log("[MaskCard] Cannot interact - Not player's turn");
            return false;
        }
        
        if (mask == null)
        {
            Debug.LogWarning("[MaskCard] Cannot interact - No mask assigned");
            return false;
        }
        
        if (playerResourceManager == null || !playerResourceManager.HasResource(ResourceType.ActionPoint, mask.SwitchCost))
        {
            Debug.Log($"[MaskCard] Cannot interact - Insufficient action points (need {mask.SwitchCost}, have {playerResourceManager?.CurrentActionPoints ?? 0})");
            return false;
        }
        
        targetUnit = FindNearestPlayerUnit();
        
        if (targetUnit == null)
        {
            Debug.Log("[MaskCard] Cannot interact - No valid friendly unit in range");
            return false;
        }
        
        if (targetUnit.Controller == null)
        {
            Debug.LogWarning("[MaskCard] Cannot interact - Target unit has no controller");
            return false;
        }
        
        Debug.Log($"[MaskCard] Valid match found with unit: {targetUnit.gameObject.name}");
        return true;
    }
    
    protected override void afterMatch()
    {
        if (targetUnit != null && targetUnit.Controller != null && mask != null)
        {
            ActionCommand switchCommand = new ActionCommand(targetUnit.Controller, ActionType.SwitchMask)
            {
                MaskData = mask,
                ResourceCost = mask.SwitchCost
            };
            
            if (targetUnit.Controller.CanPerformAction(switchCommand))
            {
                targetUnit.Controller.ConfirmAction(switchCommand);
                
                Debug.Log($"[MaskCard] Successfully submitted SwitchMask command for {targetUnit.gameObject.name}");
                
                if (handManager != null)
                {
                    handManager.RemoveCard(gameObject);
                }
                
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[MaskCard] Cannot perform SwitchMask action on {targetUnit.gameObject.name}");
                StartCoroutine(ReturnBackAction());
            }
        }
        else
        {
            Debug.LogWarning("[MaskCard] afterMatch failed - Invalid state");
            StartCoroutine(ReturnBackAction());
        }
    }
    
    private BattleUnit FindNearestPlayerUnit()
    {
        if (roundManager == null) return null;
        
        BattleUnit nearestUnit = null;
        float nearestDistance = validInteractionDistance;
        
        foreach (var unit in roundManager.battleUnits)
        {
            if (unit == null || !unit.IsAlive()) continue;
            
            if (unit.UnitTeam == Team.Player)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestUnit = unit;
                }
            }
        }
        
        return nearestUnit;
    }
}
