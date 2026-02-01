using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 面具启效果的黄圈交互
/// 右键点击可以触发面具的Activate效果
/// 支持有序更新以节省性能
/// </summary>
public class ActivateCircle : MonoBehaviour, IPointerClickHandler
{
    private BattleUnit boundUnit;
    private SpriteRenderer spriteRenderer;
    private bool isActive = true;
    
    public void Initialize(BattleUnit unit)
    {
        boundUnit = unit;
        
        // 设置到UI Layer，让EventSystem可以检测到
        gameObject.layer = LayerMask.NameToLayer("UI");
        
        // 获取预制体上的SpriteRenderer（应该已经存在）
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("[ActivateCircle] 预制体上缺少SpriteRenderer组件！");
            return;
        }
        
        // 设置黄色半透明效果
        //spriteRenderer.color = new Color(1f, 1f, 0f, 0.5f);
        spriteRenderer.sortingOrder = 0;
        
        // 禁用Physics碰撞体（如果存在），避免干扰ActionCircle
        CircleCollider2D physicsCollider = GetComponent<CircleCollider2D>();
        if (physicsCollider != null)
        {
            physicsCollider.enabled = false;
            Debug.Log("[ActivateCircle] 禁用Physics碰撞体，使用EventSystem检测");
        }
        
        // 确保有BoxCollider2D作为EventSystem的目标（如果没有就添加）
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D eventCollider = gameObject.AddComponent<BoxCollider2D>();
            eventCollider.isTrigger = true;
            // 根据sprite大小设置
            if (spriteRenderer.sprite != null)
            {
                eventCollider.size = spriteRenderer.sprite.bounds.size;
            }
        }
        
        // 位置已在BattleUnit.ShowActivateCircle()中设置为(0, -1.7, 0)
        // 不在这里修改，保持BattleUnit中设置的位置
        
        Debug.Log($"[ActivateCircle] 为 {unit.gameObject.name} 初始化黄圈（EventSystem模式），位置: {transform.localPosition}");
        
        // 注册到管理器
        if (ActivateCircleManager.Instance != null)
        {
            ActivateCircleManager.Instance.RegisterCircle(this);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 使用EventSystem检测右键点击（不受Physics collider影响）
        if (eventData.button == PointerEventData.InputButton.Right && isActive)
        {
            Debug.Log("[ActivateCircle] EventSystem检测到右键点击");
            TriggerActivate();
        }
    }
    
    private void TriggerActivate()
    {
        if (boundUnit == null || boundUnit.Controller == null)
        {
            Debug.LogWarning("[ActivateCircle] BattleUnit 或 Controller 为空");
            return;
        }
        
        Mask mask = boundUnit.CurrentMask;
        if (mask == null || !mask.CanUseActivate)
        {
            Debug.LogWarning("[ActivateCircle] Mask 为空或 CanUseActivate 为 false");
            return;
        }
        
        Debug.Log($"[ActivateCircle] 触发 {mask.MaskName} 的启效果");
        
        // 设置为不可用
        mask.CanUseActivate = false;
        isActive = false;
        
        // 提交Activate协程到AnimationHandler
        AnimationHandler animHandler = AnimationHandler.Instance;
        if (animHandler != null)
        {
            IEnumerator activateCoroutine = mask.Activate(boundUnit.Controller);
            // 创建一个ActivateMask的ActionCommand
            ActionCommand activateCommand = new ActionCommand(boundUnit.Controller, ActionType.ActivateMask)
            {
                ResourceCost = 0,
                MaskData = mask
            };
            animHandler.SubmitAction(activateCoroutine, activateCommand, boundUnit.Controller);
        }
        else
        {
            Debug.LogWarning("[ActivateCircle] AnimationHandler 未找到，直接执行协程");
            StartCoroutine(mask.Activate(boundUnit.Controller));
        }
        
        // 销毁黄圈
        Destroy(gameObject);
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
    }
    
    private void OnDestroy()
    {
        // 从管理器注销
        if (ActivateCircleManager.Instance != null)
        {
            ActivateCircleManager.Instance.UnregisterCircle(this);
        }
        
        // 清理协程
        StopAllCoroutines();
    }
}
