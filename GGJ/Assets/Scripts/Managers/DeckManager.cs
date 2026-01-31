using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    // 单例实例
    public static DeckManager Instance;

    [Header("卡组配置")]
    [Tooltip("逆卡巴拉计数器")] public int bodyCultCount = 3;
    [Tooltip("呀！")] public int hilichurlCount = 2;
    [Tooltip("牛战士从不摘下自己的面具！")] public int neverRemovedCount = 2;
    [Tooltip("你被火焰包围了")] public int flameCount = 2;
    [Tooltip("开始电力运输")] public int endfieldCount = 3;
    [Tooltip("折磨开始！")] public int agonyCount = 2;
    [Tooltip("秉烛人")] public int candleHandlerCount = 2;
    [Tooltip("母鸡卡的世界观！")] public int oblivionisCount = 2;

    // 卡池
    private List<Type> cardPool = new List<Type>();

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            InitDeck(); // 初始化卡组
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化卡组：根据配置数量填充卡池
    /// </summary>
    public void InitDeck()
    {
        cardPool.Clear();

        // 按配置数量添加对应面具类型到卡池
        AddMaskToPool(typeof(MaskBodyCult), bodyCultCount);
        AddMaskToPool(typeof(MaskHilichurl), hilichurlCount);
        AddMaskToPool(typeof(MaskNeverRemoved), neverRemovedCount);
        AddMaskToPool(typeof(MaskFlame), flameCount);
        AddMaskToPool(typeof(MaskEndfield), endfieldCount);
        AddMaskToPool(typeof(MaskAgony), agonyCount);
        AddMaskToPool(typeof(MaskCandleHandler), candleHandlerCount);
        AddMaskToPool(typeof(MaskOblivionis), oblivionisCount);

        Debug.Log($"[DeckManager] 卡组初始化完成，总卡数：{cardPool.Count}");
    }

    /// <summary>
    /// 批量添加指定类型的面具到卡池
    /// </summary>
    /// <param name="maskType">Mask子类的Type</param>
    /// <param name="count">添加数量</param>
    private void AddMaskToPool(Type maskType, int count)
    {
        if (!typeof(Mask).IsAssignableFrom(maskType))
        {
            Debug.LogError($"[DeckManager] {maskType.Name} 不是Mask的子类！");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            cardPool.Add(maskType);
        }
    }

    /// <summary>
    /// 抽卡核心方法：从卡池随机抽取一张，返回Mask实例（卡池移除该卡）
    /// </summary>
    /// <returns>抽到的Mask实例（null表示卡池为空）</returns>
    public Mask DrawMask()
    {
        if (cardPool.Count == 0)
        {
            Debug.LogWarning("[DeckManager] 卡池已空，无法抽卡！");
            return null;
        }

        // 随机选一个面具类型
        int randomIndex = UnityEngine.Random.Range(0, cardPool.Count);
        Type drawnMaskType = cardPool[randomIndex];
        cardPool.RemoveAt(randomIndex); // 从卡池移除（避免重复抽）

        // 实例化对应的Mask子类
        Mask drawnMask = Activator.CreateInstance(drawnMaskType) as Mask;
        if (drawnMask == null)
        {
            Debug.LogError($"[DeckManager] 实例化 {drawnMaskType.Name} 失败！");
            return DrawMask(); // 递归重试（可选，也可返回null）
        }

        Debug.Log($"[DeckManager] 抽卡成功：{drawnMask.MaskName}，剩余卡数：{cardPool.Count}");
        return drawnMask;
    }

    /// <summary>
    /// 重置卡组（清空并重新初始化）
    /// </summary>
    public void ResetDeck()
    {
        InitDeck();
        Debug.Log("[DeckManager] 卡组已重置");
    }

    /// <summary>
    /// 获取卡池剩余数量（外部查询用）
    /// </summary>
    public int GetRemainingCardCount()
    {
        return cardPool.Count;
    }
}