# 分层架构
    基础层：Mask基类，实现核心装备逻辑
    效果层：MaskEffect抽象类和各种具体效果实现
    状态层：StatusEffect系统处理战斗状态
    Unity层：MonoBehaviour包装，便于编辑器配置
# 

# 关键特性实现
    效果优先级：通过EffectTiming枚举和触发顺序控制
    面具切换：处理旧面具销毁效果，再应用新面具
    耐久系统：优先消耗耐久，溢出伤害传递到HP
    状态系统：可扩展的状态效果框架
# 

# 使用方法
    // 创建面具实例
    var unbearableMask = new UnbearableMask();
    var flameMask = new FlameMaidenMask();

    // 玩家装备面具
    playerUnit.EquipMask(unbearableMask);

    // 战斗中切换面具（触发销毁效果）
    playerUnit.EquipMask(flameMask);

    // 触发面具效果
    unbearableMask.TriggerEffects(EffectTiming.OnEquip, battleSystem);

    // 检查切换消耗
    var (apCost, mpCost) = mask.GetSwitchCost();
# 