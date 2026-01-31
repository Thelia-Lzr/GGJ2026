using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }
    private void Awake() 
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
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard(); // 空格抽卡测试*******************************************************
    }

    public void DrawCard()
    {
        // 1. 手牌上限
        if (handCards.Count >= maxHandSize) return;

        // 2. 调用DeckManager抽卡，校验卡池是否为空
        CardType? drawnCardType = DeckManager.Instance?.DrawCardType();
        if (!drawnCardType.HasValue) return;

        // 3. 生成卡牌预制体
        GameObject g = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        g.name = $"Card_{drawnCardType.Value}"; // 可选：给卡牌命名，方便调试识别类型

        // 4. 禁用碰撞
        Collider2D col = g.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 5. 加入手牌列表+更新位置
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
}