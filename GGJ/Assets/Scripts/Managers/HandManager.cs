using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using TMPro;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }
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
    private List<Mask> deck = new();
    private List<Mask> discardPile = new();
    
    private PlayerResourceManager playerResourceManager => PlayerResourceManager.Instance;

    private void Start()
    {
        InitializeDeck();
        ShuffleDeck();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (RoundManager.Instance != null && 
                RoundManager.Instance.GetActiveTeam() == Team.Player && 
                RoundManager.Instance.IsBattleActive() &&
                DragController.Instance != null &&
                DragController.Instance.Status == 0)
            {
                TryDrawCard();
            }
        }
    }

    private void InitializeDeck()
    {
        deck.Clear();
        
        // 火焰少女的面具 - 4张
        for (int i = 0; i < 4; i++)
            deck.Add(new MaskFlame());
        
        // 难崩的假面 - 3张
        for (int i = 0; i < 3; i++)
            deck.Add(new MaskOblivionis());
        
        // 拉线工人的面罩 - 4张
        for (int i = 0; i < 4; i++)
            deck.Add(new MaskEndfield());
        
        // 从不摘下的面具 - 3张
        for (int i = 0; i < 3; i++)
            deck.Add(new MaskNeverRemoved());
        
        // 痛苦的面具 - 3张
        for (int i = 0; i < 3; i++)
            deck.Add(new MaskAgony());
        
        // 人体派的面具 - 3张
        for (int i = 0; i < 3; i++)
            deck.Add(new MaskBodyCult());
        
        // 丘丘人的面具 - 2张
        for (int i = 0; i < 2; i++)
            deck.Add(new MaskHilichurl());
        
        // 秉烛人的面具 - 3张
        for (int i = 0; i < 3; i++)
            deck.Add(new MaskCandleHandler());
        
        Debug.Log($"[HandManager] Deck initialized with {deck.Count} cards");
    }
    
    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Mask temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        
        Debug.Log("[HandManager] Deck shuffled");
    }

    public void TryDrawCard()
    {
        if (playerResourceManager == null)
        {
            Debug.LogWarning("[HandManager] PlayerResourceManager not found");
            return;
        }
        
        if (!playerResourceManager.HasResource(ResourceType.ActionPoint, 1))
        {
            Debug.Log("[HandManager] Not enough action points to draw card");
            return;
        }
        
        if (playerResourceManager.SpendResource(ResourceType.ActionPoint, 1))
        {
            Debug.Log("[HandManager] Spent 1 AP to draw card");
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize)
        {
            Debug.Log("[HandManager] Hand is full, cannot draw card");
            return;
        }
        
        if (deck.Count == 0)
        {
            Debug.Log("[HandManager] Deck is empty, reshuffling discard pile");
            ReshuffleDiscardPile();
            
            if (deck.Count == 0)
            {
                Debug.LogWarning("[HandManager] No cards available to draw");
                return;
            }
        }
        
        Mask drawnMask = deck[0];
        deck.RemoveAt(0);
        
        GameObject cardObj = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        
        MaskCard maskCard = cardObj.GetComponent<MaskCard>();
        if (maskCard != null)
        {
            maskCard.Mask = drawnMask;
            Debug.Log($"[HandManager] Drew card: {drawnMask.MaskName}");
        }
        else
        {
            Debug.LogError("[HandManager] CardPrefab does not have MaskCard component!");
        }
        
        Collider2D col = cardObj.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        handCards.Add(cardObj);
        UpdateCardPositions();
    }
    
    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("[HandManager] Discard pile is also empty");
            return;
        }
        
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        
        Debug.Log($"[HandManager] Reshuffled discard pile into deck. Deck now has {deck.Count} cards");
    }
    
    public void DiscardCard(Mask mask)
    {
        if (mask != null)
        {
            discardPile.Add(mask);
            Debug.Log($"[HandManager] Discarded: {mask.MaskName}");
        }
    }
    
    public void RemoveCard(GameObject card)
    {
        if (handCards.Remove(card))
        {
            UpdateCardPositions();
        }
    }
    
    /// <summary>
    /// 公开方法：重新整理手牌位置
    /// 用于在卡牌返回手牌或其他需要更新位置的情况
    /// </summary>
    public void RefreshCardPositions()
    {
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
            
            // 将本地空间坐标转换为世界空间坐标
            Vector3 localPosition = spline.EvaluatePosition(p);
            Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);
            
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            // 为了在Lambda表达式中安全使用，缓存当前卡牌变量
            GameObject currentCard = handCards[i];
            
            // 检查卡牌是否存在
            if (currentCard == null)
            {
                Debug.LogWarning($"[HandManager] Card at index {i} is null, skipping");
                continue;
            }

            // 处理层级 - 设置基础 sorting order
            // 从第 3 层开始（0-2 层留给背景等其他元素），每张卡占用 3 层
            int baseSortingOrder = i * 3 + 3;
            
            // 设置根对象的 SpriteRenderer（卡牌背景）
            var sr = currentCard.GetComponent<SpriteRenderer>();
            if (sr) sr.sortingOrder = baseSortingOrder;
            
            // 设置所有子对象的 sorting order
            SetChildrenSortingOrder(currentCard, baseSortingOrder);

            // 检查是否有动画正在进行到完全不同的位置（可能是拖动后的返回）
            // 只有在这种情况下才 DOKill，避免动画堆积
            float distanceToTarget = Vector3.Distance(currentCard.transform.position, worldPosition);
            if (distanceToTarget > 0.5f) // 如果距离目标位置超过0.5单位，说明可能有冲突的动画
            {
                currentCard.transform.DOKill(); // 只在必要时才杀死动画
            }

            // 移动动画 - DOTween 会自动优雅地覆盖同类型的动画
            currentCard.transform.DOMove(worldPosition, 0.25f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    if (currentCard != null) // 安全检查
                    {
                        Collider2D col = currentCard.GetComponent<Collider2D>();
                        if (col != null) col.enabled = true;
                    }
                });

            // 旋转动画 - DOTween 会自动优雅地覆盖同类型的动画
            currentCard.transform.DOLocalRotateQuaternion(rotation, 0.25f)
                .SetEase(Ease.OutQuad);
        }
    }
    
    private void SetChildrenSortingOrder(GameObject card, int baseSortingOrder)
    {
        // 获取所有子对象的 SpriteRenderer 并设置 sorting order
        SpriteRenderer[] childSprites = card.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in childSprites)
        {
            // 跳过根对象（已经设置过了）
            if (sprite.gameObject == card) continue;
            
            sprite.sortingOrder = baseSortingOrder + 1;
        }
        
        // 获取所有子对象的 TextMeshPro 并设置 sorting order
        TextMeshPro[] childTexts = card.GetComponentsInChildren<TextMeshPro>();
        foreach (var text in childTexts)
        {
            var renderer = text.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = baseSortingOrder + 2;
            }
        }
    }
    
    // Debug information
    public int GetDeckCount() => deck.Count;
    public int GetHandCount() => handCards.Count;
    public int GetDiscardCount() => discardPile.Count;
}
