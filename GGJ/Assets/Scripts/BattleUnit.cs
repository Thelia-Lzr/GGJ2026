using System.Collections.Generic;
using UnityEngine;
using System;

public enum Team
{
    Player,
    Enemy
}

public class BattleUnit : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentEnergy;
    [SerializeField] private Team team;
    
    [Header("References")]
    private UnitController controller;
    private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    
    public event Action<int> OnHealthChanged;
    public event Action<int> OnEnergyChanged;
    public event Action<StatusEffect> OnStatusApplied;
    public event Action<StatusEffect> OnStatusRemoved;
    public event Action OnDeath;
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;
    
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int MaxEnergy => maxEnergy;
    public int CurrentEnergy => currentEnergy;
    public Team UnitTeam => team;
    public UnitController Controller => controller;
    public IReadOnlyList<StatusEffect> ActiveStatusEffects => activeStatusEffects.AsReadOnly();
    
    public void Initialize(UnitController unitController)
    {
        controller = unitController;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        activeStatusEffects.Clear();
    }
    
    public void ApplyDamage(int amount)
    {
        if (!IsAlive()) return;
        
        int actualDamage = Mathf.Max(0, amount);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (!IsAlive()) return;
        
        int actualHeal = Mathf.Max(0, amount);
        currentHealth = Mathf.Min(maxHealth, currentHealth + actualHeal);
        
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void ModifyEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy);
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
