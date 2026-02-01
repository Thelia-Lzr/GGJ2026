using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActivateCircle管理器
/// 统一管理所有ActivateCircle的更新，有序更新以节省性能
/// </summary>
public class ActivateCircleManager : MonoBehaviour
{
    public static ActivateCircleManager Instance { get; private set; }
    
    [Header("预制体配置")]
    [Tooltip("ActivateCircle预制体（启效果黄圈）")]
    [SerializeField] private GameObject activateCirclePrefab;
    
    private List<ActivateCircle> activeCircles = new List<ActivateCircle>();
    private int currentUpdateIndex = 0;
    private int circlesPerFrame = 2; // 每帧更新的黄圈数量
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 获取ActivateCircle预制体
    /// </summary>
    public GameObject GetActivateCirclePrefab()
    {
        if (activateCirclePrefab == null)
        {
            Debug.LogError("[ActivateCircleManager] ActivateCircle预制体未设置！请在Inspector中拖拽赋值。");
        }
        return activateCirclePrefab;
    }
    
    public void RegisterCircle(ActivateCircle circle)
    {
        if (!activeCircles.Contains(circle))
        {
            activeCircles.Add(circle);
            Debug.Log($"[ActivateCircleManager] 注册ActivateCircle，当前总数: {activeCircles.Count}");
        }
    }
    
    public void UnregisterCircle(ActivateCircle circle)
    {
        if (activeCircles.Remove(circle))
        {
            Debug.Log($"[ActivateCircleManager] 注销ActivateCircle，当前总数: {activeCircles.Count}");
        }
    }
    
    private void Update()
    {
        if (activeCircles.Count == 0)
            return;
        
        // 有序更新：每帧只更新部分圆圈
        int updatedCount = 0;
        int startIndex = currentUpdateIndex;
        
        while (updatedCount < circlesPerFrame && activeCircles.Count > 0)
        {
            if (currentUpdateIndex >= activeCircles.Count)
            {
                currentUpdateIndex = 0;
            }
            
            if (currentUpdateIndex < activeCircles.Count)
            {
                ActivateCircle circle = activeCircles[currentUpdateIndex];
                
                // 检查圆圈是否还有效
                if (circle == null || !circle.gameObject.activeInHierarchy)
                {
                    activeCircles.RemoveAt(currentUpdateIndex);
                    continue;
                }
                
                // 更新圆圈（如果需要可以调用自定义更新方法）
                // circle.UpdateVisuals(); // 如果ActivateCircle有自定义更新方法
                
                currentUpdateIndex++;
                updatedCount++;
            }
            
            // 防止无限循环
            if (currentUpdateIndex == startIndex && updatedCount == 0)
                break;
        }
    }
    
    public void ClearAllCircles()
    {
        foreach (var circle in activeCircles)
        {
            if (circle != null)
            {
                Destroy(circle.gameObject);
            }
        }
        
        activeCircles.Clear();
        currentUpdateIndex = 0;
        Debug.Log("[ActivateCircleManager] 清除所有ActivateCircle");
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
