using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    private static PlayerResourceManager instance;
    public static PlayerResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerResourceManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PlayerResourceManager");
                    instance = go.AddComponent<PlayerResourceManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Action Points")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int currentActionPoints = 3;
    
    [Header("Mask System - TCG Style")]
    [SerializeField] private List<Mask> deck = new List<Mask>();
    [SerializeField] private List<Mask> hand = new List<Mask>();
    [SerializeField] private List<Mask> discardPile = new List<Mask>();
    [SerializeField] private int maxHandSize = 5;
    
    public event Action<int> OnActionPointsChanged;
    public event Action<Mask> OnMaskDrawn;
    public event Action<Mask> OnMaskDiscarded;
    public event Action OnDeckShuffled;
    public event Action OnDeckReshuffled;
    public event Action OnHandChanged;
    
    public int MaxActionPoints => maxActionPoints;
    public int CurrentActionPoints => currentActionPoints;
    public int MaxHandSize => maxHandSize;
    public IReadOnlyList<Mask> Hand => hand.AsReadOnly();
    public IReadOnlyList<Mask> Deck => deck.AsReadOnly();
    public IReadOnlyList<Mask> DiscardPile => discardPile.AsReadOnly();
    public int DeckCount => deck.Count;
    public int HandCount => hand.Count;
    public int DiscardPileCount => discardPile.Count;
    
    private void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        ResetResources();
    }

    private void InitializeResources()
    {
    }
    
    public bool HasResource(ResourceType type, int amount)
    {
        if (type == ResourceType.ActionPoint)
        {
            return currentActionPoints >= amount;
        }
        return false;
    }
    
    public bool SpendResource(ResourceType type, int amount)
    {
        if (type == ResourceType.ActionPoint)
        {
            if (!HasResource(type, amount))
                return false;
            
            currentActionPoints = Mathf.Max(0, currentActionPoints - amount);
            OnActionPointsChanged?.Invoke(currentActionPoints);
            return true;
        }
        return false;
    }
    
    public void GainResource(ResourceType type, int amount)
    {
        if (type == ResourceType.ActionPoint)
        {
            currentActionPoints = Mathf.Min(maxActionPoints, currentActionPoints + amount);
            OnActionPointsChanged?.Invoke(currentActionPoints);
        }
    }
    
    public int GetResource(ResourceType type)
    {
        if (type == ResourceType.ActionPoint)
        {
            return currentActionPoints;
        }
        return 0;
    }
    
    public void SetResource(ResourceType type, int value)
    {
        if (type == ResourceType.ActionPoint)
        {
            currentActionPoints = Mathf.Clamp(value, 0, maxActionPoints);
            OnActionPointsChanged?.Invoke(currentActionPoints);
        }
    }
    
    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
        OnActionPointsChanged?.Invoke(currentActionPoints);
    }
    
    public void InitializeDeck(List<Mask> initialMasks)
    {
        deck.Clear();
        hand.Clear();
        discardPile.Clear();
        
        if (initialMasks != null)
        {
            deck.AddRange(initialMasks);
            ShuffleDeck();
        }
    }
    
    public void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Mask temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        
        OnDeckShuffled?.Invoke();
    }
    
    public void DrawMasks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!DrawMask())
            {
                break;
            }
        }
    }
    
    public bool DrawMask()
    {
        if (hand.Count >= maxHandSize)
        {
            Debug.LogWarning("Hand is full! Cannot draw more masks.");
            return false;
        }
        
        if (deck.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.LogWarning("No masks available to draw!");
                return false;
            }
            
            ReshuffleDeck();
        }
        
        if (deck.Count > 0)
        {
            Mask drawnMask = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnMask);
            
            OnMaskDrawn?.Invoke(drawnMask);
            OnHandChanged?.Invoke();
            return true;
        }
        
        return false;
    }
    
    public void DiscardMask(Mask mask)
    {
        if (mask == null || !hand.Contains(mask))
            return;
        
        hand.Remove(mask);
        discardPile.Add(mask);
        
        OnMaskDiscarded?.Invoke(mask);
        OnHandChanged?.Invoke();
    }
    
    public void DiscardHand()
    {
        while (hand.Count > 0)
        {
            Mask mask = hand[0];
            hand.RemoveAt(0);
            discardPile.Add(mask);
            OnMaskDiscarded?.Invoke(mask);
        }
        
        OnHandChanged?.Invoke();
    }
    
    public void ReshuffleDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        
        OnDeckReshuffled?.Invoke();
    }
    
    public void AddMaskToDeck(Mask mask)
    {
        if (mask == null)
            return;
        
        deck.Add(mask);
    }
    
    public void AddMaskToHand(Mask mask)
    {
        if (mask == null || hand.Count >= maxHandSize)
            return;
        
        hand.Add(mask);
        OnHandChanged?.Invoke();
    }
    
    public bool HasMaskInHand(Mask mask)
    {
        return hand.Contains(mask);
    }
    
    public bool CanDrawMask()
    {
        return hand.Count < maxHandSize && (deck.Count > 0 || discardPile.Count > 0);
    }
    
    public void ResetResources()
    {
        ResetActionPoints();
    }
}
