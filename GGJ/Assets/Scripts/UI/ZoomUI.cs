using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ZoomUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] float zoomSize = 1.5f;     // 放大倍数
    [SerializeField] float animDuration = 0.25f;// 动画时间
    [SerializeField] int hoverSortingOrder = 100;// 悬停时的层级

    private Vector3 originalScale;
    private Quaternion originalRotation; 
    private int originalSortingOrder;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. 记录状态
        originalSortingOrder = spriteRenderer.sortingOrder;
        originalRotation = transform.localRotation; 

        // 2. 提升层级（防止被旁边的卡遮挡）
        spriteRenderer.sortingOrder = hoverSortingOrder;

        // 3. 执行动画
        transform.DOKill(); // 打断之前的动画

        // 变大
        transform.DOScale(originalScale * zoomSize, animDuration).SetEase(Ease.OutBack);

        transform.DOLocalRotate(Vector3.zero, animDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 1. 恢复层级
        spriteRenderer.sortingOrder = originalSortingOrder;

        // 2. 执行动画
        transform.DOKill();

        // 恢复大小
        transform.DOScale(originalScale, animDuration).SetEase(Ease.OutQuad);

        // 恢复旋转：回到刚才记录的倾斜角
        transform.DOLocalRotateQuaternion(originalRotation, animDuration).SetEase(Ease.OutQuad);
    }
}