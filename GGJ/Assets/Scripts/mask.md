# 分层架构
    基础层：Mask基类，实现核心装备逻辑
    效果层：MaskEffect抽象类和各种具体效果实现
    状态层：StatusEffect系统处理战斗状态
    Unity层：MonoBehaviour包装，便于编辑器配置
# 

# 效果系统优化
    处理优先级：销毁时 > 佩戴时 > 启动 > 永续
    记述优先级：佩戴时 > 启动 > 永续 > 销毁时（在UI显示时使用）
    预实现了所有示例效果
#

# 关键特性实现
    效果优先级：通过EffectTiming枚举和触发顺序控制
    面具切换：处理旧面具销毁效果，再应用新面具
    耐久系统：优先消耗耐久，溢出伤害传递到HP
    状态系统：可扩展的状态效果框架
# 

# 卡牌管理系统
    完整的牌库、手牌、弃牌堆管理
    洗牌、抽牌、弃牌逻辑
    手牌拖拽佩戴机制
# 

# Unity集成
    MaskUIController：处理UI显示和拖拽
    CharacterController：角色接收面具
    BattleManagerUnity：战斗场景管理
#

# 使用方法
    // 创建面具实例
    var unbearableMask = UnbearableMask.Create();
    var flameMask = FlameMaidenMask.Create();

    // 初始化卡组
    var cardManager = new CardManager();
    cardManager.InitializeDeck(new List<Mask> { unbearableMask, flameMask });

    // 抽牌
    cardManager.DrawCard(); // 抽到难绷假面

    // 从手牌佩戴面具到角色
    cardManager.PlayMaskFromHand(unbearableMask, playerUnit, battleManager);
    // 这会触发：1.卸下旧面具 2.旧面具销毁效果 3.佩戴新面具 4.新面具佩戴效果
# 