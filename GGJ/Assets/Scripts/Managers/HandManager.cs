using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) DrawCard();
    }

    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize) return;
        GameObject g = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
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
            handCards[i].transform.DOMove(splinePosition, 0.25f);
            handCards[i].transform.DOLocalRotateQuaternion(rotation, 0.25f);
        }
    }
}
