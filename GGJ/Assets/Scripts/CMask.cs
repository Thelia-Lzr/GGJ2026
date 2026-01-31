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
    
    // /// <summary>
    // /// 状态类型
    // /// </summary>
    // public enum StatusType
    // {
    //     Enraged,        // 暴怒
    //     Stunned,        // 眩晕
    //     Disarmed,       // 缴械
    //     Confused,       // 混乱
    //     Weakened        // 虚弱
    // }

    /// <summary>
    /// 卡片位置
    /// </summary>
     public enum CardLocation
    {
        Hand,           // 手牌
        InPlay,         // 场上
        Discard,        // 弃牌堆
        Deck            // 牌库
    }

    // ==================== 状态系统 ====================
    
    // /// <summary>
    // /// 状态效果基类
    // /// </summary>
    // [System.Serializable]
    // public abstract class StatusEffect
    // {
    //     public StatusType Type { get; protected set; }
    //     public int Duration { get; protected set; }
    //     public BattleUnit Target { get; protected set; }
        
    //     public virtual void Apply(BattleUnit target)
    //     {
    //         Target = target;
    //         OnApply();
    //     }
        
    //     public virtual void OnTurnStart() { }
    //     public virtual void OnTurnEnd() { }
    //     public virtual void OnAttack(ref int damage) { }
    //     public virtual void OnAttacked(ref int damage) { }
    //     public virtual void OnRemove() { }
        
    //     protected abstract void OnApply();
        
    //     public bool Tick()
    //     {
    //         Duration--;
    //         if (Duration <= 0)
    //         {
    //             OnRemove();
    //             return true;
    //         }
    //         return false;
    //     }
    // }
    
    // /// <summary>
    // /// 暴怒状态：攻击力翻倍
    // /// </summary>
    // public class EnragedStatus : StatusEffect
    // {
    //     private float multiplier = 2f;
    //     private int originalAttack;
        
    //     public EnragedStatus(int duration = 2)
    //     {
    //         Type = StatusType.Enraged;
    //         Duration = duration;
    //     }
        
    //     protected override void OnApply()
    //     {
    //         originalAttack = Target.BaseAttack;
    //         Target.BaseAttack = Mathf.RoundToInt(originalAttack * multiplier);
    //     }
        
    //     public override void OnRemove()
    //     {
    //         Target.BaseAttack = originalAttack;
    //     }
    // }
    
    // /// <summary>
    // /// 眩晕状态：跳过行动
    // /// </summary>
    // public class StunnedStatus : StatusEffect
    // {
    //     public StunnedStatus(int duration = 1)
    //     {
    //         Type = StatusType.Stunned;
    //         Duration = duration;
    //     }
        
    //     protected override void OnApply()
    //     {
    //         // 对于敌人：替换下一个动作为眩晕
    //         // 对于玩家：无法使用启动效果
    //     }
        
    //     public override void OnTurnStart()
    //     {
    //         if (Target is PlayerUnit player)
    //         {
    //             player.CanActivate = false;
    //         }
    //     }
    // }

    // ==================== 面具效果基类 ====================
    
    /// <summary>
    /// 面具效果接口
    /// </summary>
    public interface IMaskEffect
    {
        string Description { get; }
        EffectTriggerTiming Timing { get; }

        void Trigger(Mask mask, BattleUnit owner, BattleSystem context);
        bool CheckCondition(Mask mask,BattleUnit owner, BattleSystem context);
    }
    
    /// <summary>
    /// 抽象基类：面具效果
    /// </summary>
    [System.Serializable]
    public abstract class MaskEffect : IMaskEffect
    {
        [SerializeField] protected string description;
        [SerializeField] protected EffectTriggerTiming timing;

        public string Description => description;
        public EffectTriggerTiming Timing => timing;
        
        public virtual void Trigger(Mask mask, BattleUnit owner, BattleSystem context)
        {
            if (context != null)
            {
                Execute(mask, owner, context);
            }
        }
        
        public virtual bool CheckCondition(Mask mask, BattleUnit owner, BattleManager context)
        {
            return true;
        }
        
        protected abstract void Execute(Mask mask, BattleUnit owner, BattleSystem context);
    }
    

    // ==================== 具体效果实现 ====================

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
            timing = EffectTriggerTiming.OnEquip;
        }
        
        protected override void Execute(Mask mask, BattleUnit owner, BattleManager context)
        {
            if (context?.CardManager != null)
            {
                for (int i = 0; i < drawCount; i++)
                {
                    context.CardManager.DrawCard();
                }
                Debug.Log($"[效果] {mask.Name} - 抽{drawCount}张牌");
            }
        }
    }
    
    /// <summary>
    /// 连击效果（永续）：第N次攻击获得额外攻击次数
    /// </summary>
    [System.Serializable]
    public class ComboAttackEffect : MaskEffect
    {
        [SerializeField] private int requiredAttacks = 3;
        [SerializeField] private int extraAttacks = 1;
        private int attackCount = 0;
        
        public ComboAttackEffect()
        {
            description = $"场上角色第{requiredAttacks}次攻击时，获得额外{extraAttacks}次攻击次数";
            timing = EffectTriggerTiming.OnCondition;
        }
        
        public override bool CheckCondition(Mask mask, BattleUnit owner, BattleManager context)
        {
            // 检查是否是场上角色的第三次攻击
            if (context != null && context.LastAttackUnit == owner)
            {
                attackCount++;
                if (attackCount % requiredAttacks == 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        protected override void Execute(Mask mask, BattleUnit owner, BattleManager context)
        {
            if (owner is PlayerUnit player)
            {
                // 给予额外攻击次数
                player.AddExtraAction(extraAttacks);
                Debug.Log($"[效果] {mask.Name} - 触发连击，获得{extraAttacks}次额外攻击");
            }
        }
    }



    /// <summary>
    /// 攻击成长效果（永续）：每次攻击攻击力+5
    /// </summary>
    [System.Serializable]
    public class AttackGrowthEffect : MaskEffect
    {
        [SerializeField] private int growthAmount = 5;
        
        public AttackGrowthEffect()
        {
            description = $"每次角色攻击时，攻击加成+{growthAmount}";
            timing = EffectTriggerTiming.OnCondition;
        }
        
        public override bool CheckCondition(Mask mask, BattleUnit owner, BattleManager context)
        {
            // 检查该角色是否进行了攻击
            return context != null && 
                   context.LastAttackUnit == owner && 
                   owner.EquippedMask == mask;
        }
        
        protected override void Execute(Mask mask, BattleUnit owner, BattleManager context)
        {
            mask.AttackBonus += growthAmount;
            Debug.Log($"[效果] {mask.Name} - 攻击加成增加至{mask.AttackBonus}");
        }
    }


/// <summary>
    /// 销毁其他面具效果（销毁时）
    /// </summary>
    [System.Serializable]
    public class DestroyOtherMaskEffect : MaskEffect
    {
        public DestroyOtherMaskEffect()
        {
            description = "销毁另一张场上的面具，抽1";
            timing = EffectTriggerTiming.OnDestroy;
        }
        
        protected override void Execute(Mask mask, BattleUnit owner, BattleManager context)
        {
            if (context?.PlayerParty == null) return;
            
            // 查找另一张场上的面具（除了自己）
            Mask otherMask = null;
            foreach (var unit in context.PlayerParty.Units)
            {
                if (unit != owner && unit.EquippedMask != null)
                {
                    otherMask = unit.EquippedMask;
                    break;
                }
            }
            
            // 销毁另一张面具
            if (otherMask != null)
            {
                otherMask.BreakMask();
                Debug.Log($"[效果] {mask.Name} - 销毁了{otherMask.Name}");
            }
            
            // 抽1张牌
            if (context.CardManager != null)
            {
                context.CardManager.DrawCard();
                Debug.Log($"[效果] {mask.Name} - 抽1张牌");
            }
        }
    }
    
    /// <summary>
    /// 群攻效果
    /// </summary>
    [System.Serializable]
    public class AoEEffect : MaskEffect
    {
        public AoEEffect()
        {
            description = "群攻：攻击所有敌人";
            timing = EffectTriggerTiming.OnCondition;
        }
        
        public override bool CheckCondition(Mask mask, BattleUnit owner, BattleManager context)
        {
            // 当角色使用该面具攻击时触发
            return context != null && 
                   context.CurrentAttackingMask == mask;
        }
        
        protected override void Execute(Mask mask, BattleUnit owner, BattleManager context)
        {
            // 群攻逻辑在战斗系统中处理 
        }
    }



    // ==================== 核心Mask类 ====================
    
    /// <summary>
    /// Mask类：遵循"卡牌"概念的实现
    /// </summary>
    [System.Serializable]
    public class Mask : IEquatable<Mask>
    {
        // 基础属性
        public string MaskID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public CardLocation Location { get; set; } = CardLocation.Deck;

        // 战斗属性（UI显示部分）
        public int AttackBonus { get; set; }
        public int Durability { get; private set; }
        public int MaxDurability { get; private set; }
        
        // 类型信息
        public MaskType Type { get; private set; }
        public DamageType DamageType { get; private set; }
        
        // 效果系统
        private List<MaskEffect> effects;
        private Dictionary<EffectTriggerTiming, List<MaskEffect>> effectMap;
        
        // 拥有者信息
        public BattleUnit EquippedBy { get; private set; }
        public bool IsEquipped => EquippedBy != null;
        
        // 事件
        public event Action<Mask> OnEquipped;
        public event Action<Mask> OnUnequipped;
        public event Action<Mask> OnDestroyed;
        public event Action<Mask> OnMovedToDiscard;
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
            
            InitializeEffectMap();
        }
        
        private void InitializeEffectMap()
        {
            effects = new List<MaskEffect>();
            effectMap = new Dictionary<EffectTriggerTiming, List<MaskEffect>>();
            
            foreach (EffectTriggerTiming timing in Enum.GetValues(typeof(EffectTriggerTiming)))
            {
                effectMap[timing] = new List<MaskEffect>();
            }
            
            // 根据伤害类型自动添加基础效果
            AddBaseEffectByDamageType();
        }
        
        /// <summary>
        /// 添加效果
        /// </summary>
        private void AddBaseEffectByDamageType()
        {
            switch (DamageType)
            {
                case DamageType.AoE:
                    AddEffect(new AoEEffect());
                    break;
                case DamageType.SingleTarget:
                    // 单体攻击不需要特殊效果
                    break;
                case DamageType.Splash:
                    // 溅射效果
                    break;
            }
        }
        
        /// <summary>
        /// 添加效果
        /// </summary>
        public void AddEffect(MaskEffect effect)
        {
            effects.Add(effect);
            effectMap[effect.Timing].Add(effect);
        }

        /// <summary>
        /// 触发指定时机的效果
        /// 处理优先级：销毁时 > 佩戴时 > 启动 > 永续
        /// </summary>
        public void TriggerEffects(EffectTriggerTiming timing, BattleManager context)
        {
            if (!effectMap.ContainsKey(timing)) return;
            
            // 按照处理优先级排序执行
            var effectsToTrigger = effectMap[timing];
            foreach (var effect in effectsToTrigger)
            {
                if (timing == EffectTiming.OnCondition)
                {
                    if (effect.CheckCondition(this, EquippedBy, context))
                    {
                        effect.Trigger(this, EquippedBy, context);
                    }
                }
                else
                {
                    effect.Trigger(this, EquippedBy, context);
                }
            }
        }
/// <summary>
        /// 佩戴面具（从手牌拖到角色）
        /// </summary>
        public bool EquipTo(BattleUnit unit, BattleManager context)
        {
            if (unit == null || Location != CardLocation.Hand)
            {
                Debug.LogWarning($"无法佩戴面具: {Name}，当前位置: {Location}");
                return false;
            }
            
            // 如果目标已经装备了面具，先处理旧面具
            if (unit.EquippedMask != null)
            {
                // 1. 弃置原先的面具
                var oldMask = unit.EquippedMask;
                oldMask.Unequip();
                
                // 2. 结算原先面具销毁时的效果
                oldMask.TriggerEffects(EffectTriggerTiming.OnDestroy, context);
                
                // 3. 将旧面具送入弃牌堆
                oldMask.MoveToDiscard(context);
            }
            
            // 装备新面具
            EquippedBy = unit;
            unit.EquippedMask = this;
            Location = CardLocation.InPlay;
            
            // 4. 结算新面具佩戴时的效果（优先级：佩戴时 > 启动 > 永续）
            OnEquipped?.Invoke(this);
            
            // 按优先级触发效果
            TriggerEffects(EffectTriggerTiming.OnEquip, context);
            
            Debug.Log($"[面具] {unit.Name} 佩戴了 {Name}");
            return true;
        }
        
        /// <summary>
        /// 卸下面具
        /// </summary>
        public void Unequip()
        {
            if (!IsEquipped) return;
            
            var previousOwner = EquippedBy;
            if (previousOwner != null && previousOwner.EquippedMask == this)
            {
                previousOwner.EquippedMask = null;
            }
            
            EquippedBy = null;
            OnUnequipped?.Invoke(this);
        }
        
        /// <summary>
        /// 面具受到伤害
        /// 敌人优先攻击耐久，耐久为0时送入弃牌堆，溢出伤害扣除血量
        /// </summary>
        public int TakeDamage(int damage, BattleManager context)
        {
            if (Durability <= 0) return damage;
            
            int previousDurability = Durability;
            int damageToDurability = Mathf.Min(Durability, damage);
            Durability -= damageToDurability;
            
            int overflowDamage = Mathf.Max(0, damage - damageToDurability);
            
            // 触发耐久度变化事件
            OnDurabilityChanged?.Invoke(this, Durability);
            
            Debug.Log($"[面具] {Name} 受到{damage}点伤害，耐久: {previousDurability}->{Durability}");
            
            // 如果面具被破坏
            if (Durability <= 0 && previousDurability > 0)
            {
                BreakMask(context);
            }
            
            return overflowDamage;
        }
        
        /// <summary>
        /// 破坏面具（耐久归零）
        /// </summary>
        public void BreakMask(BattleManager context = null)
        {
            if (Durability > 0) Durability = 0;
            
            // 触发销毁效果（最高优先级）
            TriggerEffects(EffectTriggerTiming.OnDestroy, context);
            
            // 触发事件
            OnDestroyed?.Invoke(this);
            
            // 送入弃牌堆
            MoveToDiscard(context);
            
            Debug.Log($"[面具] {Name} 被破坏");
        }
        
        /// <summary>
        /// 移动到弃牌堆
        /// </summary>
        public void MoveToDiscard(BattleManager context)
        {
            Unequip();
            Location = CardLocation.Discard;
            OnMovedToDiscard?.Invoke(this);
            
            if (context?.CardManager != null)
            {
                context.CardManager.AddToDiscardPile(this);
            }
        }
        
        /// <summary>
        /// 激活面具效果（每回合一次）
        /// </summary>
        public bool Activate(BattleManager context)
        {
            if (!IsEquipped || Location != CardLocation.InPlay) return false;
            
            TriggerEffects(EffectTriggerTiming.OnActivate, context);
            Debug.Log($"[面具] 激活 {Name} 的效果");
            return true;
        }
        
        /// <summary>
        /// 检查永续效果条件
        /// </summary>
        public void CheckConditionalEffects(BattleManager context)
        {
            TriggerEffects(EffectTriggerTiming.OnCondition, context);
        }
        
        /// <summary>
        /// 重置面具状态（用于新战斗）
        /// </summary>
        public void ResetForBattle()
        {
            Durability = MaxDurability;
        }
        
        /// <summary>
        /// 获取效果描述文本（按记述优先级）
        /// 记述优先级：佩戴时 > 启动 > 永续 > 销毁时
        /// </summary>
        public string GetFormattedEffects()
        {
            var sb = new System.Text.StringBuilder();
            
            // 先添加伤害类型
            sb.AppendLine($"攻击类型: {GetDamageTypeDescription()}");
            
            // 按记述优先级添加效果
            var orderedEffects = new List<MaskEffect>();
            
            // 佩戴时效果
            if (effectMap.TryGetValue(EffectTriggerTiming.OnEquip, out var onEquipEffects))
                orderedEffects.AddRange(onEquipEffects);
            
            // 启动效果
            if (effectMap.TryGetValue(EffectTriggerTiming.OnActivate, out var onActivateEffects))
                orderedEffects.AddRange(onActivateEffects);
            
            // 永续效果
            if (effectMap.TryGetValue(EffectTriggerTiming.OnCondition, out var onConditionEffects))
                orderedEffects.AddRange(onConditionEffects);
            
            // 销毁时效果
            if (effectMap.TryGetValue(EffectTriggerTiming.OnDestroy, out var onDestroyEffects))
                orderedEffects.AddRange(onDestroyEffects);
            
            // 添加效果描述
            foreach (var effect in orderedEffects)
            {
                sb.AppendLine(effect.Description);
            }
            
            return sb.ToString();
        }
        
        private string GetDamageTypeDescription()
        {
            return DamageType switch
            {
                DamageType.SingleTarget => "单体",
                DamageType.AoE => "群攻",
                DamageType.Splash => "溅射",
                _ => "未知"
            };
        }
        
        public override string ToString()
        {
            return $"{Name} (攻击+{AttackBonus}, 耐久:{Durability}/{MaxDurability})";
        }
        
        public bool Equals(Mask other)
        {
            return other != null && MaskID == other.MaskID;
        }
    }

    // ==================== 预定义面具 ====================
    
    /// <summary>
    /// 难绷假面
    /// </summary>
    public static class UnbearableMask
    {
        public static Mask Create()
        {
            var mask = new Mask(
                id: "MASK_001",
                name: "难绷假面",
                attackBonus: 10,
                durability: 15,
                type: MaskType.Offensive,
                damageType: DamageType.AoE  // 群攻
            );
            
            mask.AddEffect(new DrawCardEffect()); // 佩戴时：抽1
            mask.AddEffect(new ComboAttackEffect()); // 永续：第三次攻击获得额外攻击
            mask.AddEffect(new DestroyOtherMaskEffect()); // 销毁时：销毁另一张面具，抽1
            
            return mask;
        }
    }
    
    /// <summary>
    /// 火焰少女面具
    /// </summary>
    public static class FlameMaidenMask
    {
        public static Mask Create()
        {
            var mask = new Mask(
                id: "MASK_002",
                name: "火焰少女面具",
                attackBonus: 20,
                durability: 10,
                type: MaskType.Offensive,
                damageType: DamageType.SingleTarget  // 单体
            );
            
            mask.AddEffect(new AttackGrowthEffect()); // 永续：每次攻击攻击力+5
            
            return mask;
        }
    }

    // ==================== 卡牌管理器 ====================
    
    public class CardManager
    {
        private List<Mask> deck;
        private List<Mask> hand;
        private List<Mask> discardPile;
        
        public IReadOnlyList<Mask> Hand => hand.AsReadOnly();
        public int HandCount => hand.Count;
        public int DeckCount => deck.Count;
        public int DiscardCount => discardPile.Count;
        
        public CardManager()
        {
            deck = new List<Mask>();
            hand = new List<Mask>();
            discardPile = new List<Mask>();
        }
        
        /// <summary>
        /// 初始化牌库
        /// </summary>
        public void InitializeDeck(List<Mask> masks)
        {
            deck.Clear();
            deck.AddRange(masks);
            ShuffleDeck();
        }
        
        /// <summary>
        /// 洗牌
        /// </summary>
        public void ShuffleDeck()
        {
            var rng = new System.Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
        }
        
        /// <summary>
        /// 抽牌
        /// </summary>
        public Mask DrawCard()
        {
            if (deck.Count == 0)
            {
                // 牌库空时，重洗弃牌堆
                ReshuffleDiscardPile();
                if (deck.Count == 0) return null;
            }
            
            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
            card.Location = CardLocation.Hand;
            
            Debug.Log($"[抽牌] 抽到: {card.Name}, 手牌数: {hand.Count}");
            return card;
        }
        
        /// <summary>
        /// 将手牌中的面具佩戴到角色
        /// </summary>
        public bool PlayMaskFromHand(Mask mask, BattleUnit target, BattleManager context)
        {
            if (!hand.Contains(mask))
            {
                Debug.LogWarning($"面具不在手牌中: {mask.Name}");
                return false;
            }
            
            bool success = mask.EquipTo(target, context);
            if (success)
            {
                hand.Remove(mask);
            }
            
            return success;
        }
        
        /// <summary>
        /// 添加到弃牌堆
        /// </summary>
        public void AddToDiscardPile(Mask mask)
        {
            discardPile.Add(mask);
            mask.Location = CardLocation.Discard;
        }
        
        /// <summary>
        /// 将弃牌堆洗回牌库
        /// </summary>
        private void ReshuffleDiscardPile()
        {
            if (discardPile.Count == 0) return;
            
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
            
            Debug.Log($"[洗牌] 将弃牌堆洗回牌库，牌库数: {deck.Count}");
        }
        
        /// <summary>
        /// 获取手牌中所有面具
        /// </summary>
        public List<Mask> GetHandMasks()
        {
            return new List<Mask>(hand);
        }
        
        /// <summary>
        /// 移除手牌中的面具
        /// </summary>
        public void RemoveFromHand(Mask mask)
        {
            hand.Remove(mask);
        }
    }

    // ==================== 战斗相关类 ====================
    
    public class BattleManager
    {
        public CardManager CardManager { get; set; }
        public PlayerParty PlayerParty { get; set; }
        public BattleUnit LastAttackUnit { get; set; }
        public Mask CurrentAttackingMask { get; set; }
        
        // 战斗流程管理
        public void StartBattle()
        {
            // 初始化卡牌
            CardManager?.DrawCard(); // 初始抽牌
            CardManager?.DrawCard();
            CardManager?.DrawCard();
        }
        
        public void OnUnitAttack(BattleUnit attacker, Mask mask)
        {
            LastAttackUnit = attacker;
            CurrentAttackingMask = mask;
            
            // 触发永续效果检查
            foreach (var unit in PlayerParty.Units)
            {
                if (unit.EquippedMask != null)
                {
                    unit.EquippedMask.CheckConditionalEffects(this);
                }
            }
        }
    }
    
    public class PlayerParty
    {
        public List<PlayerUnit> Units { get; private set; }
        
        public PlayerParty()
        {
            Units = new List<PlayerUnit>();
        }
        
        public void AddUnit(PlayerUnit unit)
        {
            Units.Add(unit);
        }
        
        public bool AllUnitsDefeated()
        {
            foreach (var unit in Units)
            {
                if (unit.IsAlive) return false;
            }
            return true;
        }
    }
    
    public class BattleUnit
    {
        public string Name { get; protected set; }
        public int MaxHP { get; protected set; }
        public int CurrentHP { get; protected set; }
        public int BaseAttack { get; set; }
        public Mask EquippedMask { get; set; }
        public BattleManager BattleSystem { get; set; }
        
        public bool IsAlive => CurrentHP > 0;
        
        public virtual int GetTotalAttack()
        {
            int total = BaseAttack;
            if (EquippedMask != null && EquippedMask.IsEquipped)
            {
                total += EquippedMask.AttackBonus;
            }
            return total;
        }
        
        public virtual void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            
            int damageToHP = damage;
            
            // 面具耐久优先承受伤害
            if (EquippedMask != null && EquippedMask.IsEquipped)
            {
                damageToHP = EquippedMask.TakeDamage(damage, BattleSystem);
            }
            
            // 剩余伤害扣除血量
            if (damageToHP > 0)
            {
                CurrentHP = Mathf.Max(0, CurrentHP - damageToHP);
                Debug.Log($"[伤害] {Name} 受到{damageToHP}点伤害，HP: {CurrentHP}/{MaxHP}");
            }
            
            if (!IsAlive)
            {
                OnDefeated();
            }
        }
        
        protected virtual void OnDefeated()
        {
            Debug.Log($"[战斗] {Name} 被击败");
            // 如果装备了面具，面具进入弃牌堆
            if (EquippedMask != null)
            {
                EquippedMask.MoveToDiscard(BattleSystem);
            }
        }
    }
    
    public class PlayerUnit : BattleUnit
    {
        private int extraActions = 0;
        
        public PlayerUnit(string name, int maxHP, int baseAttack) 
            : base(name, maxHP, baseAttack)
        {
        }
        
        public void AddExtraAction(int count)
        {
            extraActions += count;
        }
        
        public bool HasExtraAction()
        {
            return extraActions > 0;
        }
        
        public void UseExtraAction()
        {
            if (extraActions > 0)
            {
                extraActions--;
            }
        }
        
        /// <summary>
        /// 执行攻击
        /// </summary>
        public void PerformAttack(List<BattleUnit> targets)
        {
            if (EquippedMask == null) return;
            
            int attackPower = GetTotalAttack();
            
            switch (EquippedMask.DamageType)
            {
                case DamageType.SingleTarget:
                    // 单体攻击
                    if (targets.Count > 0)
                    {
                        targets[0].TakeDamage(attackPower);
                    }
                    break;
                    
                case DamageType.AoE:
                    // 群攻：攻击所有目标
                    foreach (var target in targets)
                    {
                        target.TakeDamage(attackPower);
                    }
                    break;
                    
                case DamageType.Splash:
                    // 溅射攻击
                    if (targets.Count > 0)
                    {
                        targets[0].TakeDamage(attackPower);
                        // 对相邻目标造成一半伤害
                        // 这里需要具体的相邻逻辑
                    }
                    break;
            }
            
            // 通知战斗系统
            BattleSystem?.OnUnitAttack(this, EquippedMask);
        }
    }
}

// ==================== Unity集成部分 ====================

#if UNITY_ENGINE
namespace GGJ_MaskSystem.Unity
{
    using UnityEngine.UI;
    
    /// <summary>
    /// 面具UI控制器
    /// </summary>
    public class MaskUIController : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Text maskNameText;
        [SerializeField] private Text attackBonusText;
        [SerializeField] private Text durabilityText;
        [SerializeField] private Text effectsText;
        [SerializeField] private Image maskImage;
        
        [Header("拖拽组件")]
        [SerializeField] private DragHandler dragHandler;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private Mask currentMask;
        private CardManager cardManager;
        
        public void Initialize(Mask mask, CardManager manager)
        {
            currentMask = mask;
            cardManager = manager;
            
            UpdateUI();
            
            // 设置拖拽事件
            if (dragHandler != null)
            {
                dragHandler.OnBeginDrag += OnBeginDrag;
                dragHandler.OnDrag += OnDrag;
                dragHandler.OnEndDrag += OnEndDrag;
            }
        }
        
        private void UpdateUI()
        {
            if (currentMask == null) return;
            
            maskNameText.text = currentMask.Name;
            attackBonusText.text = $"攻击+{currentMask.AttackBonus}";
            durabilityText.text = $"{currentMask.Durability}/{currentMask.MaxDurability}";
            effectsText.text = currentMask.GetFormattedEffects();
        }
        
        private void OnBeginDrag()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.7f;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        private void OnDrag(Vector2 position)
        {
            // 更新拖拽位置
            transform.position = position;
        }
        
        private void OnEndDrag()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
            
            // 检查是否拖拽到角色上
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                Vector2.zero, 
                Mathf.Infinity, 
                LayerMask.GetMask("Character")
            );
            
            if (hit.collider != null)
            {
                var character = hit.collider.GetComponent<CharacterController>();
                if (character != null && cardManager != null)
                {
                    // 尝试佩戴面具
                    cardManager.PlayMaskFromHand(currentMask, character.Unit, FindObjectOfType<BattleManager>());
                }
            }
            
            // 返回原位
            transform.localPosition = Vector3.zero;
        }
        
        private void OnDestroy()
        {
            if (dragHandler != null)
            {
                dragHandler.OnBeginDrag -= OnBeginDrag;
                dragHandler.OnDrag -= OnDrag;
                dragHandler.OnEndDrag -= OnEndDrag;
            }
        }
    }
    
    /// <summary>
    /// 角色控制器
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private PlayerUnit unit;
        [SerializeField] private Image healthBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Transform maskSlot;
        
        public PlayerUnit Unit => unit;
        private MaskUIController equippedMaskUI;
        
        public void Initialize(PlayerUnit playerUnit)
        {
            unit = playerUnit;
            UpdateHealthUI();
        }
        
        public void EquipMask(Mask mask)
        {
            // 显示面具UI
            if (maskSlot != null && equippedMaskUI == null)
            {
                var maskUIPrefab = Resources.Load<GameObject>("Prefabs/MaskUI");
                if (maskUIPrefab != null)
                {
                    var maskObj = Instantiate(maskUIPrefab, maskSlot);
                    equippedMaskUI = maskObj.GetComponent<MaskUIController>();
                    // 这里需要更新面具UI显示
                }
            }
            
            UpdateHealthUI();
        }
        
        public void UnequipMask()
        {
            if (equippedMaskUI != null)
            {
                Destroy(equippedMaskUI.gameObject);
                equippedMaskUI = null;
            }
        }
        
        private void UpdateHealthUI()
        {
            if (healthBar != null && unit != null)
            {
                float fillAmount = (float)unit.CurrentHP / unit.MaxHP;
                healthBar.fillAmount = fillAmount;
            }
            
            if (healthText != null && unit != null)
            {
                healthText.text = $"{unit.CurrentHP}/{unit.MaxHP}";
            }
        }
        
        private void OnMouseDown()
        {
            // 角色被点击时，可以显示详细信息或准备接受面具拖拽
            Debug.Log($"选中角色: {unit.Name}");
        }
    }
    
    /// <summary>
    /// 战斗管理器（Unity版本）
    /// </summary>
    public class BattleManagerUnity : MonoBehaviour
    {
        [SerializeField] private PlayerParty playerParty;
        [SerializeField] private List<Mask> startingDeck;
        [SerializeField] private Transform handContainer;
        
        private CardManager cardManager;
        private List<MaskUIController> handControllers = new List<MaskUIController>();
        
        private void Start()
        {
            InitializeBattle();
        }
        
        private void InitializeBattle()
        {
            // 初始化卡牌管理器
            cardManager = new CardManager();
            cardManager.InitializeDeck(startingDeck);
            
            // 创建手牌UI
            for (int i = 0; i < 3; i++) // 初始抽3张
            {
                DrawCardToHand();
            }
        }
        
        private void DrawCardToHand()
        {
            var mask = cardManager.DrawCard();
            if (mask == null) return;
            
            // 创建UI
            var maskUIPrefab = Resources.Load<GameObject>("Prefabs/MaskUI");
            if (maskUIPrefab != null && handContainer != null)
            {
                var maskObj = Instantiate(maskUIPrefab, handContainer);
                var controller = maskObj.GetComponent<MaskUIController>();
                controller.Initialize(mask, cardManager);
                handControllers.Add(controller);
            }
        }
        
        /// <summary>
        /// 从手牌移除面具UI
        /// </summary>
        public void RemoveMaskFromHand(Mask mask)
        {
            var controller = handControllers.Find(c => 
                c.gameObject != null && 
                GetMaskFromController(c) == mask);
            
            if (controller != null)
            {
                handControllers.Remove(controller);
                Destroy(controller.gameObject);
            }
        }
        
        private Mask GetMaskFromController(MaskUIController controller)
        {
            // 这里需要通过反射或其他方式获取控制器中的mask
            // 简化实现
            return null;
        }
    }
    
    /// <summary>
    /// 简单的拖拽处理器
    /// </summary>
    public class DragHandler : MonoBehaviour
    {
        public event Action OnBeginDrag;
        public event Action<Vector2> OnDrag;
        public event Action OnEndDrag;
        
        private bool isDragging = false;
        private Vector2 startPosition;
        
        private void OnMouseDown()
        {
            isDragging = true;
            startPosition = transform.position;
            OnBeginDrag?.Invoke();
        }
        
        private void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                OnDrag?.Invoke(mousePosition);
            }
        }
        
        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                OnEndDrag?.Invoke();
            }
        }
    }
}
#endif