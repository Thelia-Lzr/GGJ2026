<<<<<<< Updated upstream
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// 面具基类
/// 
/// 在实现 Activate 方法时的重要规范：
/// - 始终使用传入的 controller 参数进行移动、攻击等操作
/// - 不要直接使用 equippedUnit.transform，而应该使用 controller.transform
/// - controller.BoundUnit 应该与 equippedUnit 一致
/// - 所有协程操作（MoveTo, Attack等）都应该由 controller 发起
/// </summary>
public abstract class Mask
{
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }

    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Atk { get; protected set; }

    public int AtkCost { get; protected set; } //每次使用该面具进行攻击消耗的耐久
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }

    public bool IsBroken => CurrentHealth <= 0;

    public Mask(string maskName, int switchCost,int maxHealth,int atk,int atkCost)
    {
        MaskName = maskName;
        SwitchCost = switchCost;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Atk = atk;
        AtkCost = atkCost;
    }

    public virtual void OnAddedToInventory()
    {
    }

    public virtual void OnRemovedFromInventory()
    {
    }

    public virtual bool CanUseInBattle()
    {
        return true;
    }

    public virtual void OnEquip(BattleUnit unit)
    {
        equippedUnit = unit;
    }

    public virtual void OnUnequip(BattleUnit unit)
    {
        equippedUnit = null;
    }
    public virtual IEnumerator Attack(UnitController controller, BattleUnit target)//实现面具启效果
    {
        yield return AttackSingle(controller,target);
    }

    /// <summary>
    /// 单体攻击 - 攻击单个目标
    /// </summary>
    protected IEnumerator AttackSingle(UnitController controller, BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSingle: 目标无效或已死亡");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动单体攻击，目标: {target.gameObject.name}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        int damage = controller.BoundUnit.Attack + Atk;
        target.ApplyHealthChange(-damage);
        
        yield return new WaitForSeconds(0.1f);
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 群体攻击 - 对所有敌方单位造成伤害
    /// </summary>
    protected IEnumerator AttackAOE(UnitController controller,BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackAOE: Controller 无效");
            yield break;
        }

        Team enemyTeam = controller.BoundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetAllUnitsOfTeam(enemyTeam);

        if (enemies.Count == 0)
        {
            Debug.LogWarning("[Mask] AttackAOE: 没有找到敌方单位");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动群体攻击，目标数: {enemies.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        if (target != null && target.IsAlive())
        {
            yield return controller.MoveToTarget(target, 0.3f);
        }
        
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                enemy.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 溅射攻击 - 对目标及相邻位置的敌人造成伤害
    /// </summary>
    protected IEnumerator AttackSplash(UnitController controller, BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackSplash: Controller 无效");
            yield break;
        }

        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSplash: 主目标无效或已死亡");
            yield break;
        }

        Team enemyTeam = target.UnitTeam;
        Location targetLocation = target.UnitLocation;
        List<BattleUnit> splashTargets = new List<BattleUnit>();

        splashTargets.Add(target);

        List<Location> adjacentLocations = GetAdjacentLocations(targetLocation);
        List<BattleUnit> allEnemies = GetAllUnitsOfTeam(enemyTeam);

        foreach (var enemy in allEnemies)
        {
            if (enemy != target && adjacentLocations.Contains(enemy.UnitLocation) && enemy.IsAlive())
            {
                splashTargets.Add(enemy);
            }
        }

        Debug.Log($"[Mask] {MaskName} 发动溅射攻击，主目标: {target.gameObject.name}，溅射目标数: {splashTargets.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        foreach (var splashTarget in splashTargets)
        {
            if (splashTarget.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                splashTarget.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 获取相邻位置
    /// </summary>
    private List<Location> GetAdjacentLocations(Location location)
    {
        List<Location> adjacent = new List<Location>();

        switch (location)
        {
            case Location.Up:
                adjacent.Add(Location.Middle);
                break;
            case Location.Middle:
                adjacent.Add(Location.Up);
                adjacent.Add(Location.Bottom);
                break;
            case Location.Bottom:
                adjacent.Add(Location.Middle);
                break;
        }

        return adjacent;
    }

    /// <summary>
    /// 获取指定阵营的所有存活单位
    /// </summary>
    private List<BattleUnit> GetAllUnitsOfTeam(Team team)
    {
        List<BattleUnit> units = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == team && unit.IsAlive())
                {
                    units.Add(unit);
                }
            }
        }

        return units;
    }

    public virtual IEnumerator Activate(UnitController controller)//实现面具启效果
    {
        //this.UsageAfterAttack();
        yield return null;
    }


    public int GetSwitchCost()
    {
        return SwitchCost;
    }

    public virtual void OnTurnStart()
    {
    }

    public virtual void OnTurnEnd()
    {
    }

    public virtual void UsageAfterAttack()//面具使用后耐久减少
    {
        int damage = AtkCost;
        //if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return;
    }
    public virtual int TakeDamage(int damage)//戴着面具受到伤害
    {
        if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return overflow;
    }

    public virtual void RepairMask(int amount)
    {
        if (IsBroken) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }

    public virtual void FullyRepair()
    {
        CurrentHealth = MaxHealth;
    }

    protected virtual void OnMaskBroken()
    {
        Debug.Log($"面具 {MaskName} 已破碎！");
    }
}
=======
<<<<<<< Updated upstream
<<<<<<< Updated upstream
using System.Collections.Generic;
using UnityEngine;


public abstract class Mask
{
    public string MaskId { get; protected set; }
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }
    
    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Attack => Attack;
    public int Heal => Heal; //��ߵ�Ѫ��

    public Mask(string maskId, string maskName, int switchCost)
    {
        MaskId = maskId;
        MaskName = maskName;
        SwitchCost = switchCost;
    }
    
    public virtual void OnAddedToInventory()
    {
    }
    
    public virtual void OnRemovedFromInventory()
    {
    }
    
    public virtual bool CanUseInBattle()
    {
        return true;
    }
    
    public virtual void OnEquip(BattleUnit unit)
    {
        equippedUnit = unit;
    }
    
    public virtual void OnUnequip(BattleUnit unit)
    {
        equippedUnit = null;
    }
    
    public virtual void Activate(PlayerController controller)
    {
    }
    
    public MaskAttackPattern GetAttackPattern()
    {
        return attackPattern;
    }
    
    public int GetSwitchCost()
    {
        return SwitchCost;
    }
    
    public virtual void OnTurnStart()
    {
    }
    
    public virtual void OnTurnEnd()
    {
    }
}
=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// 面具基类
/// 
/// 在实现 Activate 方法时的重要规范：
/// - 始终使用传入的 controller 参数进行移动、攻击等操作
/// - 不要直接使用 equippedUnit.transform，而应该使用 controller.transform
/// - controller.BoundUnit 应该与 equippedUnit 一致
/// - 所有协程操作（MoveTo, Attack等）都应该由 controller 发起
/// </summary>
public abstract class Mask
{
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }
    public Sprite MaskObject { get; protected set; }
    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Atk { get; protected set; }

    public int AtkCost { get; protected set; } //每次使用该面具进行攻击消耗的耐久
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }

    public bool IsBroken => CurrentHealth <= 0;

    public Mask(string maskName, int switchCost,int maxHealth,int atk,int atkCost)
    {
        MaskName = maskName;
        SwitchCost = switchCost;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Atk = atk;
        AtkCost = atkCost;
    }

    public virtual void OnAddedToInventory()
    {
    }

    public virtual void OnRemovedFromInventory()
    {
    }

    public virtual bool CanUseInBattle()
    {
        return true;
    }

    public virtual void OnEquip(BattleUnit unit)
    {
        equippedUnit = unit;
    }

    public virtual void OnUnequip(BattleUnit unit)
    {
        equippedUnit = null;
    }
    public virtual IEnumerator Attack(UnitController controller, BattleUnit target)//实现面具启效果
    {
        yield return AttackSingle(controller,target);
    }

    /// <summary>
    /// 单体攻击 - 攻击单个目标
    /// </summary>
    protected IEnumerator AttackSingle(UnitController controller, BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSingle: 目标无效或已死亡");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动单体攻击，目标: {target.gameObject.name}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        int damage = controller.BoundUnit.Attack + Atk;
        target.ApplyHealthChange(-damage);
        
        yield return new WaitForSeconds(0.1f);
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 群体攻击 - 对所有敌方单位造成伤害
    /// </summary>
    protected IEnumerator AttackAOE(UnitController controller,BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackAOE: Controller 无效");
            yield break;
        }

        Team enemyTeam = controller.BoundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetAllUnitsOfTeam(enemyTeam);

        if (enemies.Count == 0)
        {
            Debug.LogWarning("[Mask] AttackAOE: 没有找到敌方单位");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动群体攻击，目标数: {enemies.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        if (target != null && target.IsAlive())
        {
            yield return controller.MoveToTarget(target, 0.3f);
        }
        
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                enemy.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 溅射攻击 - 对目标及相邻位置的敌人造成伤害
    /// </summary>
    protected IEnumerator AttackSplash(UnitController controller, BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackSplash: Controller 无效");
            yield break;
        }

        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSplash: 主目标无效或已死亡");
            yield break;
        }

        Team enemyTeam = target.UnitTeam;
        Location targetLocation = target.UnitLocation;
        List<BattleUnit> splashTargets = new List<BattleUnit>();

        splashTargets.Add(target);

        List<Location> adjacentLocations = GetAdjacentLocations(targetLocation);
        List<BattleUnit> allEnemies = GetAllUnitsOfTeam(enemyTeam);

        foreach (var enemy in allEnemies)
        {
            if (enemy != target && adjacentLocations.Contains(enemy.UnitLocation) && enemy.IsAlive())
            {
                splashTargets.Add(enemy);
            }
        }

        Debug.Log($"[Mask] {MaskName} 发动溅射攻击，主目标: {target.gameObject.name}，溅射目标数: {splashTargets.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        foreach (var splashTarget in splashTargets)
        {
            if (splashTarget.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                splashTarget.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 获取相邻位置
    /// </summary>
    private List<Location> GetAdjacentLocations(Location location)
    {
        List<Location> adjacent = new List<Location>();

        switch (location)
        {
            case Location.Up:
                adjacent.Add(Location.Middle);
                break;
            case Location.Middle:
                adjacent.Add(Location.Up);
                adjacent.Add(Location.Bottom);
                break;
            case Location.Bottom:
                adjacent.Add(Location.Middle);
                break;
        }

        return adjacent;
    }

    /// <summary>
    /// 获取指定阵营的所有存活单位
    /// </summary>
    private List<BattleUnit> GetAllUnitsOfTeam(Team team)
    {
        List<BattleUnit> units = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == team && unit.IsAlive())
                {
                    units.Add(unit);
                }
            }
        }

        return units;
    }

    public virtual IEnumerator Activate(UnitController controller)//实现面具启效果
    {
        //this.UsageAfterAttack();
        yield return null;
    }


    public int GetSwitchCost()
    {
        return SwitchCost;
    }

    public virtual void OnTurnStart()
    {
    }

    public virtual void OnTurnEnd()
    {
    }

    public virtual void UsageAfterAttack()//面具使用后耐久减少
    {
        int damage = AtkCost;
        //if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return;
    }
    public virtual int TakeDamage(int damage)//戴着面具受到伤害
    {
        if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return overflow;
    }

    public virtual void RepairMask(int amount)
    {
        if (IsBroken) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }

    public virtual void FullyRepair()
    {
        CurrentHealth = MaxHealth;
    }

    protected virtual void OnMaskBroken()
    {
        Debug.Log($"面具 {MaskName} 已破碎！");
    }
}
>>>>>>> Stashed changes
=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// 面具基类
/// 
/// 在实现 Activate 方法时的重要规范：
/// - 始终使用传入的 controller 参数进行移动、攻击等操作
/// - 不要直接使用 equippedUnit.transform，而应该使用 controller.transform
/// - controller.BoundUnit 应该与 equippedUnit 一致
/// - 所有协程操作（MoveTo, Attack等）都应该由 controller 发起
/// </summary>
public abstract class Mask
{
    public string MaskName { get; protected set; }
    public string Description { get; protected set; }
    public int SwitchCost { get; protected set; }
    public Sprite MaskIcon { get; protected set; }
    public Sprite MaskObject { get; protected set; }
    protected MaskAttackPattern attackPattern;
    protected BattleUnit equippedUnit;

    public int Atk { get; protected set; }

    public int AtkCost { get; protected set; } //每次使用该面具进行攻击消耗的耐久
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }

    public bool IsBroken => CurrentHealth <= 0;

    public Mask(string maskName, int switchCost,int maxHealth,int atk,int atkCost)
    {
        MaskName = maskName;
        SwitchCost = switchCost;
        MaxHealth = 50;
        CurrentHealth = MaxHealth;
        Atk = atk;
        AtkCost = atkCost;
    }

    public virtual void OnAddedToInventory()
    {
    }

    public virtual void OnRemovedFromInventory()
    {
    }

    public virtual bool CanUseInBattle()
    {
        return true;
    }

    public virtual void OnEquip(BattleUnit unit)
    {
        equippedUnit = unit;
    }

    public virtual void OnUnequip(BattleUnit unit)
    {
        equippedUnit = null;
    }
    public virtual IEnumerator Attack(UnitController controller, BattleUnit target)//实现面具启效果
    {
        yield return AttackSingle(controller,target);
    }

    /// <summary>
    /// 单体攻击 - 攻击单个目标
    /// </summary>
    protected IEnumerator AttackSingle(UnitController controller, BattleUnit target)
    {
        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSingle: 目标无效或已死亡");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动单体攻击，目标: {target.gameObject.name}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        int damage = controller.BoundUnit.Attack + Atk;
        target.ApplyHealthChange(-damage);
        
        yield return new WaitForSeconds(0.1f);
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 群体攻击 - 对所有敌方单位造成伤害
    /// </summary>
    protected IEnumerator AttackAOE(UnitController controller,BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackAOE: Controller 无效");
            yield break;
        }

        Team enemyTeam = controller.BoundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetAllUnitsOfTeam(enemyTeam);

        if (enemies.Count == 0)
        {
            Debug.LogWarning("[Mask] AttackAOE: 没有找到敌方单位");
            yield break;
        }

        Debug.Log($"[Mask] {MaskName} 发动群体攻击，目标数: {enemies.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        if (target != null && target.IsAlive())
        {
            yield return controller.MoveToTarget(target, 0.3f);
        }
        
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                enemy.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 溅射攻击 - 对目标及相邻位置的敌人造成伤害
    /// </summary>
    protected IEnumerator AttackSplash(UnitController controller, BattleUnit target)
    {
        if (controller == null || controller.BoundUnit == null)
        {
            Debug.LogWarning("[Mask] AttackSplash: Controller 无效");
            yield break;
        }

        if (target == null || !target.IsAlive())
        {
            Debug.LogWarning("[Mask] AttackSplash: 主目标无效或已死亡");
            yield break;
        }

        Team enemyTeam = target.UnitTeam;
        Location targetLocation = target.UnitLocation;
        List<BattleUnit> splashTargets = new List<BattleUnit>();

        splashTargets.Add(target);

        List<Location> adjacentLocations = GetAdjacentLocations(targetLocation);
        List<BattleUnit> allEnemies = GetAllUnitsOfTeam(enemyTeam);

        foreach (var enemy in allEnemies)
        {
            if (enemy != target && adjacentLocations.Contains(enemy.UnitLocation) && enemy.IsAlive())
            {
                splashTargets.Add(enemy);
            }
        }

        Debug.Log($"[Mask] {MaskName} 发动溅射攻击，主目标: {target.gameObject.name}，溅射目标数: {splashTargets.Count}");

        Vector2 originalPosition = controller.transform.position;
        
        yield return controller.MoveToTarget(target, 0.3f);
        
        foreach (var splashTarget in splashTargets)
        {
            if (splashTarget.IsAlive())
            {
                int damage = controller.BoundUnit.Attack + Atk;
                splashTarget.ApplyHealthChange(-damage);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return controller.MoveTo(originalPosition, 0.3f);
    }

    /// <summary>
    /// 获取相邻位置
    /// </summary>
    private List<Location> GetAdjacentLocations(Location location)
    {
        List<Location> adjacent = new List<Location>();

        switch (location)
        {
            case Location.Up:
                adjacent.Add(Location.Middle);
                break;
            case Location.Middle:
                adjacent.Add(Location.Up);
                adjacent.Add(Location.Bottom);
                break;
            case Location.Bottom:
                adjacent.Add(Location.Middle);
                break;
        }

        return adjacent;
    }

    /// <summary>
    /// 获取指定阵营的所有存活单位
    /// </summary>
    private List<BattleUnit> GetAllUnitsOfTeam(Team team)
    {
        List<BattleUnit> units = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == team && unit.IsAlive())
                {
                    units.Add(unit);
                }
            }
        }

        return units;
    }

    public virtual IEnumerator Activate(UnitController controller)//实现面具启效果
    {
        //this.UsageAfterAttack();
        yield return null;
    }


    public int GetSwitchCost()
    {
        return SwitchCost;
    }

    public virtual void OnTurnStart()
    {
    }

    public virtual void OnTurnEnd()
    {
    }

    public virtual void UsageAfterAttack()//面具使用后耐久减少
    {
        int damage = AtkCost;
        //if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return;
    }
    public virtual int TakeDamage(int damage)//戴着面具受到伤害
    {
        if (IsBroken) return damage;

        int actualDamage = Mathf.Min(CurrentHealth, damage);
        CurrentHealth -= actualDamage;

        int overflow = damage - actualDamage;

        if (IsBroken)
        {
            OnMaskBroken();
        }

        return overflow;
    }

    public virtual void RepairMask(int amount)
    {
        if (IsBroken) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }

    public virtual void FullyRepair()
    {
        CurrentHealth = MaxHealth;
    }

    protected virtual void OnMaskBroken()
    {
        Debug.Log($"面具 {MaskName} 已破碎！");
    }
}
>>>>>>> Stashed changes
>>>>>>> Stashed changes
