using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    BodyCult,       // 逆卡巴拉计数器
    Hilichurl,      // 呀！
    NeverRemoved,   // 牛战士从不摘下自己的面具！
    Flame,          // 你被火焰包围了
    Endfield,       // 开始电力运输
    Agony,          // 折磨开始！
    CandleHandler,  // 秉烛人
    Oblivionis      // 母鸡卡的世界观！
}

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("卡组配置-8种卡数量")]
    public int bodyCultCount = 3;
    public int hilichurlCount = 2;
    public int neverRemovedCount = 2;
    public int flameCount = 2;
    public int endfieldCount = 3;
    public int agonyCount = 2;
    public int candleHandlerCount = 2;
    public int oblivionisCount = 2;

    // 抽卡卡池（抽一张移除一张，确保不重复）
    private List<CardType> cardPool = new List<CardType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitCardPool(); // 初始化卡池
        }
    }

    /// <summary>
    /// 初始化卡池：根据配置数量填充8种卡
    /// </summary>
    private void InitCardPool()
    {
        cardPool.Clear();
        AddCardToPool(CardType.BodyCult, bodyCultCount);
        AddCardToPool(CardType.Hilichurl, hilichurlCount);
        AddCardToPool(CardType.NeverRemoved, neverRemovedCount);
        AddCardToPool(CardType.Flame, flameCount);
        AddCardToPool(CardType.Endfield, endfieldCount);
        AddCardToPool(CardType.Agony, agonyCount);
        AddCardToPool(CardType.CandleHandler, candleHandlerCount);
        AddCardToPool(CardType.Oblivionis, oblivionisCount);

        Debug.Log($"卡组初始化完成，总卡数：{cardPool.Count}");
    }

    /// <summary>
    /// 批量添加指定类型的卡到卡池
    /// </summary>
    private void AddCardToPool(CardType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            cardPool.Add(type);
        }
    }

    /// <summary>
    /// 抽卡方法：从卡池随机抽一张，返回卡类型（卡池自动移除）
    /// </summary>
    /// <returns>抽到的卡类型，卡池空则返回null</returns>
    public CardType? DrawCardType()
    {
        if (cardPool.Count == 0)
        {
            Debug.LogWarning("卡组已抽完，无卡可抽！");
            return null;
        }

        // 随机抽取并移除
        int randomIndex = Random.Range(0, cardPool.Count);
        CardType drawnType = cardPool[randomIndex];
        cardPool.RemoveAt(randomIndex);

        Debug.Log($"抽到卡：{drawnType}，卡池剩余：{cardPool.Count}");
        return drawnType;
    }

    /// <summary>
    /// 重置卡组（重新初始化卡池，可在战斗结束/重开时调用）
    /// </summary>
    public void ResetDeck()
    {
        InitCardPool();
        Debug.Log("卡组已重置");
    }
}