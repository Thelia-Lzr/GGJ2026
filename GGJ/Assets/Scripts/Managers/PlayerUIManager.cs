using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    public TextMeshProUGUI actPoint;
    
    private void Start()
    {
        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance.OnActionPointsChanged += OnActionPointsChanged;
            UpdateActionPointsUI(PlayerResourceManager.Instance.CurrentActionPoints);
        }
    }
    
    private void OnDestroy()
    {
        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance.OnActionPointsChanged -= OnActionPointsChanged;
        }
    }
    
    private void OnActionPointsChanged(int currentActionPoints)
    {
        UpdateActionPointsUI(currentActionPoints);
    }
    
    private void UpdateActionPointsUI(int actionPoints)
    {
        if (actPoint != null)
        {
            actPoint.text = $"AP: {actionPoints}/{PlayerResourceManager.Instance.MaxActionPoints}";
        }
    }
}
