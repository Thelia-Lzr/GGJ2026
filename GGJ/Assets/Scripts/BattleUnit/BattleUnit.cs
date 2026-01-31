using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public enum Team
{
    Player,
    Enemy
}

public enum Location
{
    Up,
    Middle,
    Bottom
}
public class BattleUnit : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attack = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] public Team team;
    [SerializeField] public Location location;
    
    [Header("References")]
    public UnitController controller;
    public ResourceController resourceController=>ResourceController.Instance;
    private Mask currentMask;
    private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    
    public event Action<int> OnHealthChanged;
    public event Action<StatusEffect> OnStatusApplied;
    public event Action<StatusEffect> OnStatusRemoved;
    public event Action OnDeath;
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;
    public event Action<Mask> OnMaskChanged;
    //文本
    private TMP_FontAsset textFont => ResourceController.Instance.FONT;
    //显示
    [field: SerializeField]
    public GameObject UIText { get; private set; }
    //血量文本
    [field:SerializeField]
    public GameObject HealthText {  get; private set; }
    //护盾文本
    [field: SerializeField]
    public GameObject ShellText { get; private set; }
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Attack => attack;
    public int Defense => defense;
    public Team UnitTeam => team;
    public Location UnitLocation => location;
    public UnitController Controller => controller;
    public Mask CurrentMask => currentMask;
    public IReadOnlyList<StatusEffect> ActiveStatusEffects => activeStatusEffects.AsReadOnly();
    public void addAttack(int add)
    {
        attack += add;
    }
    public void Start()
    {
        
    }
    public void Initialize(UnitController unitController, int MaxHealth,int CurrentHealth,int Atk,int Def)
    {
        controller = unitController;
        currentHealth = CurrentHealth;
        maxHealth = MaxHealth;
        attack = Atk;
        defense = Def;
        activeStatusEffects.Clear();
        //UI
        OnHealthChanged += HealthDisplay;
        OnHealthChanged += HealthChangeDisplay;
        if (UIText == null)
        {
            UIText = new GameObject("uitext");
            RectTransform transform= UIText.AddComponent<RectTransform>();
            transform.SetParent(resourceController.GetPrefab("Canvas").GetComponent<RectTransform>());
        }
        UIText.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(transform.position);
        UIText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        UIText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        UIText.GetComponent<RectTransform>().localScale = Vector3.one;
        UIText.GetComponent<RectTransform>().sizeDelta =new Vector2(250,300);
        HealthDisplay(0);
    }
    
    public void SetMask(Mask mask)
    {
        currentMask = mask;
        OnMaskChanged?.Invoke(mask);
    }
    
    public void ClearMask()
    {
        currentMask = null;
        OnMaskChanged?.Invoke(null);
    }
    public virtual void ApplyHealthChange(int amount)
    {
        if (!IsAlive()) return;
        currentHealth = Mathf.Max(0, currentHealth + amount);

        OnHealthChanged?.Invoke(amount);


        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void HealthDisplay(int amount)
    {
        if (HealthText == null)
        {
            if (team == Team.Player)
            {
                //图片
                if (true)
                {
                    GameObject healthIcon = new GameObject("HealthIcon");
                    RectTransform textTranform = healthIcon.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(0, 0);
                    textTranform.sizeDelta = new Vector2(50, 50);
                    textTranform.localScale = Vector3.one;
                    Image image = healthIcon.AddComponent<Image>();
                    image.sprite = resourceController.Sprites[0];
                }
                if (true)
                {
                    GameObject newText = new GameObject("healthText");
                    RectTransform textTranform = newText.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(50, 0);
                    textTranform.sizeDelta = new Vector2(200, 50);
                    textTranform.localScale = Vector3.one;
                    TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 50;
                    text.font = textFont;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    HealthText = newText;
                }
                //图片
                if (true)
                {
                    GameObject healthIcon = new GameObject("HealthIcon");
                    RectTransform textTranform = healthIcon.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(130, 0);
                    textTranform.sizeDelta = new Vector2(50, 50);
                    textTranform.localScale = Vector3.one;
                    Image image = healthIcon.AddComponent<Image>();
                    image.sprite = resourceController.Sprites[2];
                }
                if (true)
                {
                    GameObject newText = new GameObject("shellText");
                    RectTransform textTranform = newText.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(180, 0);
                    textTranform.sizeDelta = new Vector2(200, 50);
                    textTranform.localScale = Vector3.one;
                    TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 50;
                    text.font = textFont;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    ShellText = newText;
                }
            }
            else
            {
                //图片
                if (true)
                {
                    GameObject healthIcon = new GameObject("HealthIcon");
                    RectTransform textTranform = healthIcon.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(0, 0);
                    textTranform.sizeDelta = new Vector2(50, 50);
                    textTranform.localScale = Vector3.one;
                    Image image = healthIcon.AddComponent<Image>();
                    image.sprite = resourceController.Sprites[0];
                }
                if (true)
                {
                    GameObject newText = new GameObject("healthText");
                    RectTransform textTranform = newText.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(50, 0);
                    textTranform.sizeDelta = new Vector2(200, 50);
                    textTranform.localScale = Vector3.one;
                    TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 50;
                    text.font = textFont;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    HealthText = newText;
                }
                //图片
                if (true)
                {
                    GameObject healthIcon = new GameObject("HealthIcon");
                    RectTransform textTranform = healthIcon.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(130, 0);
                    textTranform.sizeDelta = new Vector2(50, 50);
                    textTranform.localScale = Vector3.one;
                    Image image = healthIcon.AddComponent<Image>();
                    image.sprite = resourceController.Sprites[2];
                }
                if (true)
                {
                    GameObject newText = new GameObject("shellText");
                    RectTransform textTranform = newText.AddComponent<RectTransform>();
                    textTranform.SetParent(UIText.GetComponent<RectTransform>());
                    textTranform.pivot = new Vector2(0, 1);
                    textTranform.anchorMin = new Vector2(0, 1);
                    textTranform.anchorMax = new Vector2(0, 1);
                    textTranform.anchoredPosition = new Vector2(180, 0);
                    textTranform.sizeDelta = new Vector2(200, 50);
                    textTranform.localScale = Vector3.one;
                    TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 50;
                    text.font = textFont;
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    ShellText = newText;
                }
            }
            
        }
        HealthText.GetComponent<TextMeshProUGUI>().text = CurrentHealth.ToString();
        if (currentMask != null)
        {
            ShellText.GetComponent<TextMeshProUGUI>().text = currentMask.CurrentHealth.ToString();

        }
        else
        {
            ShellText.GetComponent<TextMeshProUGUI>().text = "" + 0;
        }
    }
    public void HealthChangeDisplay(int amount)
    {
        GameObject newDisplay = Instantiate(resourceController.GetPrefab("HealthChangeDisplay"));
        newDisplay.GetComponent<RectTransform>().SetParent(UIText.GetComponent<RectTransform>());
        newDisplay.GetComponent<HealthChangeDisplay>().Intial(amount);
    }
    public void ApplyStatus(StatusEffect effect)
    {
        if (effect == null) return;
        
        StatusEffect existingEffect = activeStatusEffects.Find(e => e.StatusId == effect.StatusId);
        
        if (existingEffect != null)
        {
            existingEffect.RefreshOrStack(effect);
        }
        else
        {
            activeStatusEffects.Add(effect);
            effect.OnApplied(this);
        }
        
        OnStatusApplied?.Invoke(effect);
    }
    
    public void RemoveStatus(StatusEffect effect)
    {
        if (activeStatusEffects.Remove(effect))
        {
            effect.OnRemoved(this);
            OnStatusRemoved?.Invoke(effect);
        }
    }
    
    public bool HasStatus(string statusId)
    {
        return activeStatusEffects.Exists(e => e.StatusId == statusId);
    }
    
    public StatusEffect GetStatus(string statusId)
    {
        return activeStatusEffects.Find(e => e.StatusId == statusId);
    }
    
    public bool HasStatus<T>() where T : StatusEffect
    {
        return activeStatusEffects.Exists(e => e is T);
    }
    
    public T GetStatus<T>() where T : StatusEffect
    {
        return activeStatusEffects.Find(e => e is T) as T;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public Team GetTeam()
    {
        return team;
    }
    
    public void SetTeam(Team newTeam)
    {
        team = newTeam;
    }
    
    public void OnTurnStart()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            activeStatusEffects[i].OnTurnStart(this);
            
            if (activeStatusEffects[i].ShouldRemove())
            {
                RemoveStatus(activeStatusEffects[i]);
            }
        }
        
        OnTurnStarted?.Invoke();
    }
    
    public void OnTurnEnd()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            activeStatusEffects[i].OnTurnEnd(this);
            
            if (activeStatusEffects[i].ShouldRemove())
            {
                RemoveStatus(activeStatusEffects[i]);
            }
        }
        
        OnTurnEnded?.Invoke();
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
    }
}
