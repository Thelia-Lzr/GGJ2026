using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ZoomUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] float zoomSize = 1.5f;     // 放大倍数
    [SerializeField] float animDuration = 0.25f;// 动画时间
    [SerializeField] int sortingOrderBoost = 100; // sorting order 提升值（确保绝对在最上层）

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private int originalRootSortingOrder;
    private Dictionary<SpriteRenderer, int> originalSpriteSortingOrders = new Dictionary<SpriteRenderer, int>();
    private Dictionary<Renderer, int> originalTextSortingOrders = new Dictionary<Renderer, int>();
    
    private bool isDragging = false; // 是否正在拖动，拖动时不响应鼠标事件
    private bool isZoomed = false;   // 当前是否处于放大状态

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return; // 拖动时不响应
        
        // 记录当前旋转状态
        originalRotation = transform.localRotation;
        
        // 记录并提升 sorting order
        SaveAndBoostSortingOrders();

        // 执行动画 - DOTween 会自动优雅地覆盖之前的动画
        // 变大
        transform.DOScale(originalScale * zoomSize, animDuration).SetEase(Ease.OutBack);

        // 回正旋转
        transform.DOLocalRotate(Vector3.zero, animDuration).SetEase(Ease.OutBack);
        
        isZoomed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return; // 拖动时不响应
        
        RestoreToOriginal();
    }
    
    /// <summary>
    /// 通知 ZoomUI 开始拖动（不使用 ForceRestore）
    /// </summary>
    public void OnDragStart()
    {
        isDragging = true;
        
        // 如果当前处于放大状态，温和地恢复到原始状态
        if (isZoomed)
        {
            RestoreToOriginal();
        }
    }
    
    /// <summary>
    /// 通知 ZoomUI 拖动结束
    /// </summary>
    public void OnDragEnd()
    {
        isDragging = false;
        // 不需要做其他事情，如果鼠标还在卡牌上，OnPointerEnter 会自动触发
    }
    
    /// <summary>
    /// 温和地恢复到原始状态
    /// </summary>
    private void RestoreToOriginal()
    {
        // DOTween 会自动优雅地覆盖之前的动画，不需要 DOKill
        
        // 恢复大小
        transform.DOScale(originalScale, animDuration).SetEase(Ease.OutQuad);

        // 恢复旋转
        transform.DOLocalRotateQuaternion(originalRotation, animDuration).SetEase(Ease.OutQuad);
        
        // 恢复 sorting order
        RestoreSortingOrders();
        
        isZoomed = false;
    }
    
    private void SaveAndBoostSortingOrders()
    {
        originalSpriteSortingOrders.Clear();
        originalTextSortingOrders.Clear();
        
        // 保存并提升根对象的 SpriteRenderer
        SpriteRenderer rootSprite = GetComponent<SpriteRenderer>();
        if (rootSprite != null)
        {
            originalRootSortingOrder = rootSprite.sortingOrder;
            rootSprite.sortingOrder += sortingOrderBoost;
        }
        
        // 保存并提升所有子对象的 SpriteRenderer
        SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in childSprites)
        {
            if (sprite.gameObject == gameObject) continue; // 跳过根对象
            
            originalSpriteSortingOrders[sprite] = sprite.sortingOrder;
            sprite.sortingOrder += sortingOrderBoost;
        }
        
        // 保存并提升所有子对象的 TextMeshPro
        TextMeshPro[] childTexts = GetComponentsInChildren<TextMeshPro>();
        foreach (var text in childTexts)
        {
            Renderer renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalTextSortingOrders[renderer] = renderer.sortingOrder;
                renderer.sortingOrder += sortingOrderBoost;
            }
        }
    }
    
    private void RestoreSortingOrders()
    {
        // 恢复根对象的 SpriteRenderer
        SpriteRenderer rootSprite = GetComponent<SpriteRenderer>();
        if (rootSprite != null)
        {
            rootSprite.sortingOrder = originalRootSortingOrder;
        }
        
        // 恢复所有子对象的 SpriteRenderer
        foreach (var kvp in originalSpriteSortingOrders)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sortingOrder = kvp.Value;
            }
        }
        
        // 恢复所有子对象的 TextMeshPro
        foreach (var kvp in originalTextSortingOrders)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sortingOrder = kvp.Value;
            }
        }
        
        originalSpriteSortingOrders.Clear();
        originalTextSortingOrders.Clear();
    }
}