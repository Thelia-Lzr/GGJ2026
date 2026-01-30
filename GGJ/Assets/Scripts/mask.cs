using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ_MaskSystem
{
    // ==================== 枚举定义 ====================
    
    /// <summary>
    /// 面具类型枚举
    /// </summary>
    public enum MaskType
    {
        Offensive,      // 攻击型
        Defensive,      // 防御型
        Support,        // 支援型
        Special         // 特殊型
    }
    
    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        SingleTarget,   // 单体
        AoE,           // 群体
        Splash         // 溅射
    }
    
    /// <summary>
    /// 效果触发时机
    /// </summary>
    public enum EffectTiming
    {
        OnEquip,        // 佩戴时
        OnActivate,     // 启动时（每回合）
        OnDestroy,      // 销毁时
        OnCondition     // 条件触发（永续）
    }
    
    /// <summary>
    /// 状态类型
    /// </summary>
    public enum StatusType
    {
        Enraged,        // 暴怒
        Stunned,        // 眩晕
        Disarmed,       // 缴械
        Confused,       // 混乱
        Weakened        // 虚弱
    }

    // ==================== 状态系统 ====================
    
    /// <summary>
    /// 状态效果基类
    /// </summary>
    [System.Serializable]
    public abstract class StatusEffect
    {
        public StatusType Type { get; protected set; }
        public int Duration { get; protected set; }
        public BattleUnit Target { get; protected set; }
        
        public virtual void Apply(BattleUnit target)
        {
            Target = target;
            OnApply();
        }
        
        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() { }
        public virtual void OnAttack(ref int damage) { }
        public virtual void OnAttacked(ref int damage) { }
        public virtual void OnRemove() { }
        
        protected abstract void OnApply();
        
        public bool Tick()
        {
            Duration--;
            if (Duration <= 0)
            {
                OnRemove();
                return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// 暴怒状态：攻击力翻倍
    /// </summary>
    public class EnragedStatus : StatusEffect
    {
        private float multiplier = 2f;
        private int originalAttack;
        
        public EnragedStatus(int duration = 2)
        {
            Type = StatusType.Enraged;
            Duration = duration;
        }
        
        protected override void OnApply()
        {
            originalAttack = Target.BaseAttack;
            Target.BaseAttack = Mathf.RoundToInt(originalAttack * multiplier);
        }
        
        public override void OnRemove()
        {
            Target.BaseAttack = originalAttack;
        }
    }
    
    /// <summary>
    /// 眩晕状态：跳过行动
    /// </summary>
    public class StunnedStatus : StatusEffect
    {
        public StunnedStatus(int duration = 1)
        {
            Type = StatusType.Stunned;
            Duration = duration;
        }
        
        protected override void OnApply()
        {
            // 对于敌人：替换下一个动作为眩晕
            // 对于玩家：无法使用启动效果
        }
        
        public override void OnTurnStart()
        {
            if (Target is PlayerUnit player)
            {
                player.CanActivate = false;
            }
        }
    }

    // ==================== 面具效果基类 ====================
    
    /// <summary>
    /// 面具效果接口
    /// </summary>
    public interface IMaskEffect
    {
        void Trigger(EffectTiming timing, BattleUnit owner, BattleSystem context);
        bool CheckCondition(BattleUnit owner, BattleSystem context);
    }
    
    /// <summary>
    /// 抽象基类：面具效果
    /// </summary>
    [System.Serializable]
    public abstract class MaskEffect : IMaskEffect
    {
        [SerializeField] protected string description;
        [SerializeField] protected EffectTiming triggerTiming;
        
        public virtual void Trigger(EffectTiming timing, BattleUnit owner, BattleSystem context)
        {
            if (timing == triggerTiming)
            {
                Execute(owner, context);
            }
        }
        
        public virtual bool CheckCondition(BattleUnit owner, BattleSystem context)
        {
            return true;
        }
        
        protected abstract void Execute(BattleUnit owner, BattleSystem context);
    }
    
    /// <summary>
    /// 抽卡效果
    /// </summary>
    [System.Serializable]
    public class DrawCardEffect : MaskEffect
    {
        [SerializeField] private int drawCount = 1;
        
        public DrawCardEffect()
        {
            description = $"抽{drawCount}张牌";
            triggerTiming = EffectTiming.OnEquip;
        }
        
        protected override void Execute(BattleUnit owner, BattleSystem context)
        {
            if (context != null && context.PlayerController != null)
            {
                context.PlayerController.DrawCards(drawCount);
            }
        }
    }
    
    /// <summary>
    /// 攻击增益效果（永续）
    /// </summary>
    [System.Serializable]
    public class AttackBuffEffect : MaskEffect
    {
        [SerializeField] private int bonusPerAttack = 5;
        private int attackCount = 0;
        
        public AttackBuffEffect()
        {
            description = "每次攻击后，攻击加成+5";
            triggerTiming = EffectTiming.OnCondition;
        }
        
        public override bool CheckCondition(BattleUnit owner, BattleSystem context)
        {
            // 检查是否进行了攻击
            return context.LastActionWasAttack && context.LastAttacker == owner;
        }
        
        protected override void Execute(BattleUnit owner, BattleSystem context)
        {
            if (owner.EquippedMask != null)
            {
                attackCount++;
                owner.EquippedMask.AttackBonus += bonusPerAttack;
                Debug.Log($"{owner.Name} 攻击增益叠加，当前加成: {owner.EquippedMask.AttackBonus}");
            }
        }
    }

    // ==================== 核心Mask类 ====================
    
    /// <summary>
    /// Mask类：核心装备系统
    /// </summary>
    [System.Serializable]
    public class Mask : IEquipable
    {
        // 基础属性
        public string MaskID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        
        // 战斗属性
        public int AttackBonus { get; set; }
        public int Durability { get; private set; }
        public int MaxDurability { get; private set; }
        
        // 类型信息
        public MaskType Type { get; private set; }
        public DamageType DamageType { get; private set; }
        
        // 效果系统
        [SerializeField] private List<MaskEffect> effects;
        private Dictionary<EffectTiming, List<MaskEffect>> effectMap;
        
        // 切换消耗
        public int SwitchCostAP { get; private set; } = 1; // 行动点消耗
        public int SwitchCostMP { get; private set; } = 0; // 魔法点/能量消耗
        
        // 拥有者
        public BattleUnit Owner { get; private set; }
        public bool IsEquipped => Owner != null;
        
        // 事件
        public event Action<Mask> OnEquipped;
        public event Action<Mask> OnUnequipped;
        public event Action<Mask> OnDestroyed;
        public event Action<Mask, int> OnDurabilityChanged;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public Mask(string id, string name, int attackBonus, int durability, 
                   MaskType type, DamageType damageType)
        {
            MaskID = id;
            Name = name;
            AttackBonus = attackBonus;
            Durability = durability;
            MaxDurability = durability;
            Type = type;
            DamageType = damageType;
            effects = new List<MaskEffect>();
            effectMap = new Dictionary<EffectTiming, List<MaskEffect>>();
            
            InitializeEffectMap();
        }
        
        private void InitializeEffectMap()
        {
            foreach (EffectTiming timing in Enum.GetValues(typeof(EffectTiming)))
            {
                effectMap[timing] = new List<MaskEffect>();
            }
        }
        
        /// <summary>
        /// 添加效果
        /// </summary>
        public void AddEffect(MaskEffect effect)
        {
            effects.Add(effect);
            effectMap[effect.triggerTiming].Add(effect);
        }
        
        /// <summary>
        /// 触发指定时机的效果
        /// </summary>
        public void TriggerEffects(EffectTiming timing, BattleSystem context = null)
        {
            if (!effectMap.ContainsKey(timing)) return;
            
            // 按照处理优先级排序执行
            var effectsToTrigger = effectMap[timing];
            foreach (var effect in effectsToTrigger)
            {
                if (timing == EffectTiming.OnCondition)
                {
                    if (effect.CheckCondition(Owner, context))
                    {
                        effect.Trigger(timing, Owner, context);
                    }
                }
                else
                {
                    effect.Trigger(timing, Owner, context);
                }
            }
        }
        
        // ==================== IEquipable接口实现 ====================
        
        public EquipmentType GetEquipmentType()
        {
            return EquipmentType.Mask;
        }
        
        public bool CanUseInBattle()
        {
            return Durability > 0;
        }
        
        /// <summary>
        /// 装备面具
        /// </summary>
        public bool Equip(BattleUnit unit)
        {
            if (unit == null || !CanUseInBattle()) return false;
            
            // 如果已经装备在其他单位上，先卸下
            if (IsEquipped && Owner != unit)
            {
                Unequip();
            }
            
            Owner = unit;
            
            // 触发装备事件
            OnEquipped?.Invoke(this);
            
            // 触发装备时效果
            TriggerEffects(EffectTiming.OnEquip, unit.BattleSystem);
            
            return true;
        }
        
        /// <summary>
        /// 卸下面具
        /// </summary>
        public void Unequip()
        {
            if (!IsEquipped) return;
            
            var previousOwner = Owner;
            Owner = null;
            
            // 触发卸下事件
            OnUnequipped?.Invoke(this);
            
            // 注意：销毁效果在TakeDamage中触发，不是在这里
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public int TakeDamage(int damage)
        {
            if (!IsEquipped || Durability <= 0) return damage;
            
            int previousDurability = Durability;
            Durability = Mathf.Max(0, Durability - damage);
            int overflowDamage = Mathf.Max(0, damage - previousDurability);
            
            // 触发耐久度变化事件
            OnDurabilityChanged?.Invoke(this, Durability);
            
            // 如果面具被破坏
            if (Durability <= 0 && previousDurability > 0)
            {
                OnDestroyed?.Invoke(this);
                TriggerEffects(EffectTiming.OnDestroy, Owner?.BattleSystem);
                Unequip();
            }
            
            return overflowDamage;
        }
        
        /// <summary>
        /// 恢复耐久度
        /// </summary>
        public void Repair(int amount)
        {
            int previousDurability = Durability;
            Durability = Mathf.Min(MaxDurability, Durability + amount);
            
            if (Durability != previousDurability)
            {
                OnDurabilityChanged?.Invoke(this, Durability);
            }
        }
        
        /// <summary>
        /// 获取切换消耗
        /// </summary>
        public (int apCost, int mpCost) GetSwitchCost()
        {
            return (SwitchCostAP, SwitchCostMP);
        }
        
        /// <summary>
        /// 设置切换消耗
        /// </summary>
        public void SetSwitchCost(int apCost, int mpCost = 0)
        {
            SwitchCostAP = apCost;
            SwitchCostMP = mpCost;
        }
        
        /// <summary>
        /// 获取面具提供的技能
        /// </summary>
        public List<Skill> GetSkills()
        {
            var skills = new List<Skill>();
            
            // 根据面具属性创建基础攻击技能
            var attackSkill = new Skill(
                name: $"{Name}攻击",
                description: $"使用{Name}进行攻击",
                apCost: 1,
                targetType: DamageType == DamageType.SingleTarget ? 
                    TargetType.SingleEnemy : TargetType.AllEnemies
            );
            
            skills.Add(attackSkill);
            
            // 添加面具效果对应的技能
            foreach (var effect in effects)
            {
                if (effect.triggerTiming == EffectTiming.OnActivate)
                {
                    var effectSkill = new Skill(
                        name: $"{Name}效果",
                        description: effect.description,
                        apCost: 1,
                        targetType: TargetType.Self
                    );
                    skills.Add(effectSkill);
                }
            }
            
            return skills;
        }
        
        /// <summary>
        /// 重置面具状态（用于战斗开始）
        /// </summary>
        public void ResetForBattle()
        {
            // 可以在这里重置战斗相关的临时状态
        }
        
        public override string ToString()
        {
            return $"{Name} (攻击+{AttackBonus}, 耐久:{Durability}/{MaxDurability})";
        }
    }
    
    // ==================== 具体面具实现 ====================
    
    /// <summary>
    /// 难绷假面
    /// </summary>
    public class UnbearableMask : Mask
    {
        public UnbearableMask() : base(
            id: "MASK_001",
            name: "难绷假面",
            attackBonus: 10,
            durability: 15,
            type: MaskType.Offensive,
            damageType: DamageType.AoE
        )
        {
            Description = "群攻面具，具有强大的连续攻击能力";
            
            // 添加效果
            AddEffect(new DrawCardEffect()); // 佩戴时抽1
            
            // 永续效果：第三次攻击获得额外攻击次数
            var comboEffect = new ComboAttackEffect(3, 1);
            AddEffect(comboEffect);
            
            // 销毁效果：销毁另一张场上面具并抽1
            var destroyEffect = new DestroyOtherMaskEffect();
            AddEffect(destroyEffect);
        }
    }
    
    /// <summary>
    /// 火焰少女面具
    /// </summary>
    public class FlameMaidenMask : Mask
    {
        public FlameMaidenMask() : base(
            id: "MASK_002",
            name: "火焰少女面具",
            attackBonus: 20,
            durability: 10,
            type: MaskType.Offensive,
            damageType: DamageType.SingleTarget
        )
        {
            Description = "单体攻击面具，攻击力会随着攻击次数的增加而提升";
            
            // 永续效果：每次攻击后攻击加成+5
            AddEffect(new AttackBuffEffect());
        }
    }
    
    // ==================== 战斗单位基类 ====================
    
    /// <summary>
    /// 战斗单位抽象类
    /// </summary>
    public abstract class BattleUnit
    {
        public string Name { get; protected set; }
        public int MaxHP { get; protected set; }
        public int CurrentHP { get; protected set; }
        public int BaseAttack { get; set; }
        public Mask EquippedMask { get; protected set; }
        public BattleSystem BattleSystem { get; set; }
        
        public List<StatusEffect> StatusEffects { get; protected set; }
        
        public bool IsAlive => CurrentHP > 0;
        
        protected BattleUnit(string name, int maxHP, int baseAttack)
        {
            Name = name;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            BaseAttack = baseAttack;
            StatusEffects = new List<StatusEffect>();
        }
        
        /// <summary>
        /// 装备面具
        /// </summary>
        public virtual bool EquipMask(Mask mask)
        {
            if (mask == null) return false;
            
            // 如果已经装备了面具，先处理销毁效果
            if (EquippedMask != null)
            {
                // 注意：销毁效果在TakeDamage中触发，这里只是卸下
                var oldMask = EquippedMask;
                EquippedMask.Unequip();
            }
            
            // 装备新面具
            EquippedMask = mask;
            return mask.Equip(this);
        }
        
        /// <summary>
        /// 计算总攻击力
        /// </summary>
        public virtual int GetTotalAttack()
        {
            int total = BaseAttack;
            if (EquippedMask != null)
            {
                total += EquippedMask.AttackBonus;
            }
            
            // 状态效果影响
            foreach (var status in StatusEffects)
            {
                status.OnAttack(ref total);
            }
            
            return total;
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            
            int actualDamage = damage;
            
            // 先由面具耐久度吸收伤害
            if (EquippedMask != null && EquippedMask.CanUseInBattle())
            {
                actualDamage = EquippedMask.TakeDamage(damage);
            }
            
            // 剩余伤害扣除血量
            if (actualDamage > 0)
            {
                CurrentHP = Mathf.Max(0, CurrentHP - actualDamage);
                
                // 触发状态效果
                foreach (var status in StatusEffects)
                {
                    status.OnAttacked(ref actualDamage);
                }
            }
            
            if (!IsAlive)
            {
                OnDefeated();
            }
        }
        
        protected virtual void OnDefeated()
        {
            // 单位被击败
        }
        
        /// <summary>
        /// 添加状态效果
        /// </summary>
        public void AddStatusEffect(StatusEffect effect)
        {
            // 检查是否已有相同类型的效果
            var existing = StatusEffects.Find(s => s.Type == effect.Type);
            if (existing != null)
            {
                // 刷新持续时间
                existing.Tick(); // 先消耗一回合
                effect.Apply(this);
                StatusEffects.Remove(existing);
            }
            
            effect.Apply(this);
            StatusEffects.Add(effect);
        }
        
        /// <summary>
        /// 回合开始处理
        /// </summary>
        public virtual void OnTurnStart()
        {
            // 处理状态效果
            for (int i = StatusEffects.Count - 1; i >= 0; i--)
            {
                var status = StatusEffects[i];
                status.OnTurnStart();
                
                if (status.Tick())
                {
                    StatusEffects.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// 回合结束处理
        /// </summary>
        public virtual void OnTurnEnd()
        {
            foreach (var status in StatusEffects)
            {
                status.OnTurnEnd();
            }
            
            // 触发面具的永续效果检查
            if (EquippedMask != null)
            {
                EquippedMask.TriggerEffects(EffectTiming.OnCondition, BattleSystem);
            }
        }
    }
    
    // ==================== 玩家单位 ====================
    
    public class PlayerUnit : BattleUnit
    {
        public bool CanActivate { get; set; } = true;
        public PlayerController Controller { get; set; }
        
        public PlayerUnit(string name, int maxHP, int baseAttack) 
            : base(name, maxHP, baseAttack)
        {
        }
        
        /// <summary>
        /// 激活面具效果（每回合一次）
        /// </summary>
        public bool ActivateMaskEffect()
        {
            if (!CanActivate || EquippedMask == null) return false;
            
            EquippedMask.TriggerEffects(EffectTiming.OnActivate, BattleSystem);
            CanActivate = false;
            return true;
        }
        
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            CanActivate = true; // 重置激活状态
        }
    }
    
    // ==================== 接口定义 ====================
    
    public enum EquipmentType
    {
        Mask,
        Weapon,
        Armor,
        Accessory
    }
    
    public interface IEquipable
    {
        EquipmentType GetEquipmentType();
        bool CanUseInBattle();
    }
    
    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }
    
    public class Skill
    {
        public string Name { get; }
        public string Description { get; }
        public int APCost { get; }
        public TargetType TargetType { get; }
        
        public Skill(string name, string description, int apCost, TargetType targetType)
        {
            Name = name;
            Description = description;
            APCost = apCost;
            TargetType = targetType;
        }
    }
    
    // ==================== 支持类 ====================
    
    public class BattleSystem
    {
        public PlayerController PlayerController { get; set; }
        public bool LastActionWasAttack { get; set; }
        public BattleUnit LastAttacker { get; set; }
        // ... 其他战斗系统属性
    }
    
    public class PlayerController
    {
        public void DrawCards(int count)
        {
            // 抽卡逻辑
            Debug.Log($"抽{count}张牌");
        }
    }
    
    // ==================== 特殊效果实现 ====================
    
    /// <summary>
    /// 连击效果：第N次攻击获得额外攻击次数
    /// </summary>
    [System.Serializable]
    public class ComboAttackEffect : MaskEffect
    {
        private int requiredAttacks;
        private int extraAttacks;
        private int attackCount = 0;
        
        public ComboAttackEffect(int requiredAttacks, int extraAttacks)
        {
            this.requiredAttacks = requiredAttacks;
            this.extraAttacks = extraAttacks;
            description = $"每{requiredAttacks}次攻击，获得{extraAttacks}次额外攻击";
            triggerTiming = EffectTiming.OnCondition;
        }
        
        public override bool CheckCondition(BattleUnit owner, BattleSystem context)
        {
            if (context.LastActionWasAttack && context.LastAttacker == owner)
            {
                attackCount++;
                return attackCount % requiredAttacks == 0;
            }
            return false;
        }
        
        protected override void Execute(BattleUnit owner, BattleSystem context)
        {
            // 给玩家单位添加额外行动次数
            if (owner is PlayerUnit player)
            {
                // 这里需要与行动系统集成
                Debug.Log($"{owner.Name} 触发连击，获得{extraAttacks}次额外攻击");
            }
        }
    }
    
    /// <summary>
    /// 销毁其他面具效果
    /// </summary>
    [System.Serializable]
    public class DestroyOtherMaskEffect : MaskEffect
    {
        public DestroyOtherMaskEffect()
        {
            description = "销毁另一张场上的面具，抽1张牌";
            triggerTiming = EffectTiming.OnDestroy;
        }
        
        protected override void Execute(BattleUnit owner, BattleSystem context)
        {
            if (context?.PlayerController == null) return;
            
            // 查找其他装备的面具
            // 这里需要访问战斗中的其他单位
            // 简化实现：抽1张牌
            context.PlayerController.DrawCards(1);
            
            Debug.Log("面具销毁，触发效果：抽1张牌");
        }
    }
}

// ==================== Unity MonoBehaviour 包装类 ====================

#if UNITY_ENGINE
namespace GGJ_MaskSystem.Unity
{
    /// <summary>
    /// Unity中的Mask组件
    /// </summary>
    public class MaskComponent : MonoBehaviour
    {
        [Header("基础属性")]
        [SerializeField] private string maskID;
        [SerializeField] private string maskName;
        [SerializeField] private int attackBonus;
        [SerializeField] private int durability;
        [SerializeField] private MaskType maskType;
        [SerializeField] private DamageType damageType;
        
        [Header("切换消耗")]
        [SerializeField] private int switchAPCost = 1;
        [SerializeField] private int switchMPCost = 0;
        
        [Header("效果列表")]
        [SerializeField] private List<MaskEffect> maskEffects;
        
        // 运行时实例
        private Mask _maskInstance;
        public Mask MaskInstance => _maskInstance;
        
        private void Awake()
        {
            InitializeMask();
        }
        
        private void InitializeMask()
        {
            _maskInstance = new Mask(maskID, maskName, attackBonus, durability, maskType, damageType);
            _maskInstance.SetSwitchCost(switchAPCost, switchMPCost);
            
            foreach (var effect in maskEffects)
            {
                _maskInstance.AddEffect(effect);
            }
            
            // 订阅事件
            _maskInstance.OnEquipped += OnMaskEquipped;
            _maskInstance.OnUnequipped += OnMaskUnequipped;
            _maskInstance.OnDestroyed += OnMaskDestroyed;
            _maskInstance.OnDurabilityChanged += OnMaskDurabilityChanged;
        }
        
        private void OnMaskEquipped(Mask mask)
        {
            Debug.Log($"面具 {mask.Name} 被装备");
        }
        
        private void OnMaskUnequipped(Mask mask)
        {
            Debug.Log($"面具 {mask.Name} 被卸下");
        }
        
        private void OnMaskDestroyed(Mask mask)
        {
            Debug.Log($"面具 {mask.Name} 被破坏");
            // 可以在这里播放销毁动画
        }
        
        private void OnMaskDurabilityChanged(Mask mask, int newDurability)
        {
            Debug.Log($"面具 {mask.Name} 耐久度: {newDurability}/{mask.MaxDurability}");
        }
        
        /// <summary>
        /// 在Unity编辑器中创建预设面具
        /// </summary>
        #if UNITY_EDITOR
        [ContextMenu("创建难绷假面配置")]
        private void CreateUnbearableMask()
        {
            maskID = "MASK_001";
            maskName = "难绷假面";
            attackBonus = 10;
            durability = 15;
            maskType = MaskType.Offensive;
            damageType = DamageType.AoE;
            switchAPCost = 1;
            
            maskEffects = new List<MaskEffect>
            {
                new DrawCardEffect(),
                new ComboAttackEffect(3, 1),
                new DestroyOtherMaskEffect()
            };
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        [ContextMenu("创建火焰少女面具配置")]
        private void CreateFlameMaidenMask()
        {
            maskID = "MASK_002";
            maskName = "火焰少女面具";
            attackBonus = 20;
            durability = 10;
            maskType = MaskType.Offensive;
            damageType = DamageType.SingleTarget;
            switchAPCost = 1;
            
            maskEffects = new List<MaskEffect>
            {
                new AttackBuffEffect()
            };
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
    
    /// <summary>
    /// 面具管理器（单例）
    /// </summary>
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }
        
        [SerializeField] private List<MaskComponent> availableMasks;
        private Dictionary<string, Mask> _maskDatabase;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeDatabase()
        {
            _maskDatabase = new Dictionary<string, Mask>();
            foreach (var maskComponent in availableMasks)
            {
                if (maskComponent != null && maskComponent.MaskInstance != null)
                {
                    _maskDatabase[maskComponent.MaskInstance.MaskID] = maskComponent.MaskInstance;
                }
            }
        }
        
        public Mask GetMaskByID(string maskID)
        {
            if (_maskDatabase.TryGetValue(maskID, out Mask mask))
            {
                // 返回一个副本，确保每个实例独立
                return CloneMask(mask);
            }
            return null;
        }
        
        private Mask CloneMask(Mask original)
        {
            // 这里需要实现深拷贝逻辑
            // 简化实现：创建新实例
            var clone = new Mask(
                original.MaskID,
                original.Name,
                original.AttackBonus,
                original.Durability,
                original.Type,
                original.DamageType
            );
            
            // 复制效果（需要效果也实现Clone）
            // 简化处理
            
            return clone;
        }
        
        public List<Mask> GetAllMasks()
        {
            return new List<Mask>(_maskDatabase.Values);
        }
    }
}
#endif