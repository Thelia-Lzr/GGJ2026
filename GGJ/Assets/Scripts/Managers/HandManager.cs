using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }
<<<<<<< Updated upstream
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }
    [SerializeField] private int maxHandSize;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;

    private List<GameObject> handCards = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard();
    }

    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize) return;

        GameObject g = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);

        Collider2D col = g.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;  

        handCards.Add(g);
        UpdateCardPositions();
    }

    private void UpdateCardPositions()
    {
        if (handCards.Count == 0) return;

        float cardSpacing = 1f / maxHandSize;
        float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < handCards.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            // 为了在Lambda表达式中安全使用，缓存当前卡牌变量
            GameObject currentCard = handCards[i]; 

            // 处理层级
            var sr = currentCard.GetComponent<SpriteRenderer>();
            if (sr) sr.sortingOrder = i * 10;

            // 移动动画
            currentCard.transform.DOMove(splinePosition, 0.25f)
                .OnComplete(() => {
                    Collider2D col = currentCard.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true; 
                });

            currentCard.transform.DOLocalRotateQuaternion(rotation, 0.25f);
        }
    }
=======
    // ========== 手牌UI配置 ==========
    [SerializeField] private int maxHandSize=8;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float cardSpacing = 1.5f; 
    // 手牌列表（存储卡牌GameObject + 关联的Mask实例）
    public List<GameObject> handCardObjects = new List<GameObject>();
    public List<Mask> handCardMasks = new List<Mask>();

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 抽卡UI逻辑：调用DeckManager抽卡，生成卡牌对象并加入手牌
    /// </summary>
    public void DrawCard()
    {
        // 1. 校验手牌上限
        if (handCardObjects.Count >= maxHandSize)
        {
            Debug.Log("[HandManager] 手牌已达上限，无法抽卡！");
            return;
        }

        // 2. 从DeckManager抽卡（获取Mask实例）
        Mask drawnMask = DeckManager.Instance.DrawMask();
        if (drawnMask == null) return;

        // 3. 生成卡牌GameObject
        GameObject cardObj = InstantiateCard(drawnMask);
        if (cardObj == null) return;

        // 4. 加入手牌列表
        handCardObjects.Add(cardObj);
        handCardMasks.Add(drawnMask);

        // 5. 更新手牌位置
        UpdateHandCardPositions();
    }

    /// <summary>
    /// 实例化卡牌对象，并绑定Mask数据（设置图标等）
    /// </summary>
    /// <param name="mask">抽到的Mask实例</param>
    /// <returns>生成的卡牌GameObject</returns>
    private GameObject InstantiateCard(Mask mask)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[HandManager] 卡牌预制体未赋值！");
            return null;
        }

        // 生成卡牌对象
        GameObject cardObj = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        cardObj.name = $"Card_{mask.MaskName}";

        // 禁用碰撞
        Collider2D col = cardObj.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 设置卡牌图标（使用Mask的Icon）
        SetCardSprite(cardObj, mask.MaskIcon);

        return cardObj;
    }

    /// <summary>
    /// 设置卡牌的Sprite图标
    /// </summary>
    private void SetCardSprite(GameObject cardObj, Sprite sprite)
    {
        SpriteRenderer sr = cardObj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = cardObj.AddComponent<SpriteRenderer>();

        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"[HandManager] 卡牌 {cardObj.name} 无可用图标！");
        }
    }

    /// <summary>
    /// 更新手牌位置（横向居中排列）
    /// </summary>
    private void UpdateHandCardPositions()
    {
        float startX = -(handCardObjects.Count - 1) * cardSpacing / 2; // 居中起始X

        for (int i = 0; i < handCardObjects.Count; i++)
        {
            GameObject card = handCardObjects[i];
            if (card == null) continue;

            // 计算新位置（Y/Z沿用spawnPoint）
            Vector3 newPos = new Vector3(
                startX + i * cardSpacing,
                spawnPoint.position.y,
                spawnPoint.position.z
            );
            card.transform.position = newPos;
        }
    }

    /// <summary>
    /// 移除手牌（比如使用/丢弃卡牌时）
    /// </summary>
    /// <param name="cardObj">要移除的卡牌对象</param>
    public void RemoveCard(GameObject cardObj)
    {
        int index = handCardObjects.IndexOf(cardObj);
        if (index == -1) return;

        // 移除列表数据
        handCardObjects.RemoveAt(index);
        handCardMasks.RemoveAt(index);

        // 销毁卡牌对象
        Destroy(cardObj);

        // 更新剩余手牌位置
        UpdateHandCardPositions();
        Debug.Log($"[HandManager] 移除卡牌，剩余手牌数：{handCardObjects.Count}");
    }

    /// <summary>
    /// 清空手牌（战斗结束/重置时）
    /// </summary>
    public void ClearHand()
    {
        foreach (var cardObj in handCardObjects)
        {
            if (cardObj != null) Destroy(cardObj);
        }
        handCardObjects.Clear();
        handCardMasks.Clear();
        Debug.Log("[HandManager] 手牌已清空");
    }
>>>>>>> Stashed changes
}