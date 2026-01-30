# 回合制战斗架构设计

> 目标：整理与扩充现有技术架构思路，聚焦类职责与需要实现的方法签名（不包含具体实现）。本架构分为**处理层（逻辑与数据）**与**显示层（表现与交互）**。

---

# I. 处理层 (Logic & Data Layer)
> 负责核心游戏逻辑、数据状态管理、AI决策以及事件分发。

## 1. 核心流程控制

### RoundManager（回合管理器）
**职责**：检测回合归属（友方/敌方）、交换回合、分配行动机会与资源、驱动回合流程。
**核心方法**：
- `InitializeBattle()`：初始化战斗与队伍信息。
- `StartRound()`：开始当前回合。
- `EndRound()`：结束当前回合并结算。
- `SwapRound()`：切换回合归属（友方/敌方）。
- `GrantActionPoints()`：为当前阵营分配行动资源。
- `ConsumeActionPoints(int amount)`：消耗行动资源。
- `RegisterUnit(BattleUnit unit)`：登记战斗单位。
- `UnregisterUnit(BattleUnit unit)`：移除战斗单位。
- `GetActiveTeam()`：获取当前行动阵营。
- `IsBattleOver()`：判断战斗是否结束。

### PlayerInputSystem（玩家输入系统 - 事件中心）
**职责**：采用事件驱动模型，作为逻辑层的输入事件中心。它不直接读取硬件输入，而是监听来自显示层动画处理器（AnimationHandler）的信号，并将具体的游戏意图（如“攻击”、“换装备”）分发给对应的控制器。
**核心方法**：
- `RegisterInputEvents()`：注册回调函数。
- `OnBattleActionSelected(ActionCommand command)`：触发战斗行动事件。
- `OnSwitchEquipmentRequest(Mask newMask, BattleUnit targetUnit)`：触发更换面具请求。
- `OnAcquireTemporaryMask(Mask mask)`：触发获得临时面具事件。
- `OnAssignMask(Mask mask, UnitSlot slot)`：触发面具分配事件。

---

## 2. 战斗单位与控制

### BattleUnit（战斗单位数据体）
**职责**：承载战斗单位数据与状态（HP、能量、Buff/Debuff、阵营等），作为控制器与表现的桥梁，是逻辑计算的核心实体。
**核心方法**：
- `Initialize(UnitController controller)`：绑定控制器并初始化。
- `ApplyDamage(int amount)`：承受伤害。
- `Heal(int amount)`：回复生命。
- `ApplyStatus(StatusEffect effect)`：施加状态。
- `RemoveStatus(StatusEffect effect)`：移除状态。
- `IsAlive()`：是否存活。
- `GetTeam()`：获取阵营。
- `OnTurnStart()`：回合开始钩子。
- `OnTurnEnd()`：回合结束钩子。

### UnitController（控制器基类）
**职责**：控制 BattleUnit 属性、技能与行动流程，连接数据（BattleUnit）与规则（RoundManager）。
**核心方法**：
- `BindUnit(BattleUnit unit)`：绑定战斗单位。
- `TakeTurn()`：执行自身回合逻辑入口。
- `CanAct()`：判断是否可行动（状态/资源/控制等）。
- `PerformAction(ActionCommand command)`：执行行动指令。
- `SpendResource(ResourceType type, int amount)`：消耗资源。
- `SwitchMask(Mask newMask, int cost)`：更换面具并消耗资源。
- `GetAvailableActions()`：获取可用行动列表（技能/普通攻击/道具等）。

### EnemyController（敌方控制器基类）
**职责**：继承 UnitController，通过 AI 算法自动决策。
**核心方法**：
- `AI()`：敌方 AI 决策入口。
- `SelectTarget()`：选择目标。
- `SelectAction()`：选择行动。
- `TakeTurn()`：覆写以调用 `AI()` 并执行行动。

### PlayerController（玩家控制器基类）
**职责**：继承 UnitController，接收玩家输入，通过响应 AnimationHandler 的事件来执行操作。
**核心方法**：
- `HandleInput(PlayerInput input)`：处理玩家输入。
- `Attack()`：普通攻击逻辑入口（由子类实现）。
- `UseSkill(Skill skill, Target target)`：使用技能。
- `ConfirmAction(ActionCommand command)`：确认并执行行动。
- `TakeTurn()`：覆写以进入玩家操作流程。

---

## 3. 物品与装备系统

### Mask（面具/核心装备）
**职责**：核心战斗装备，允许在战斗中切换并消耗资源。
**核心方法**：
- `GetEquipmentType()`：返回类型（Mask）。
- `OnAddedToInventory()` / `OnRemovedFromInventory()`：背包事件钩子。
- `CanUseInBattle()`：战斗可用性检查。
- `OnEquip(BattleUnit unit)`：装备时触发。
- `OnUnequip(BattleUnit unit)`：卸下时触发。
- `Activate(PlayerController controller)`：被调用时的行为入口。
- `GetSkills()`：返回面具提供的技能集合。
- `GetSwitchCost()`：获取切换消耗。

可以增加更多钩子以适配更奇怪的设计

---

# II. 显示层 (Presentation & Interaction Layer)
> 负责 UI 渲染、用户输入捕捉、拖拽交互反馈及视觉特效。

## 1. 交互组件

### DraggableUnit（可拖拽单位基类）
**设计理念**：统一处理“拖拽-释放-归位”的交互逻辑。拖拽结束时若目标无效，自动弹回原位。
**核心方法**：
- `Initialize(Vector2 startPosition)`：记录原点。
- `OnDragStart()`：开始拖拽，由 `PlayerInput` 调用。
- `OnDragEnd(Vector2 dropPosition)`：结束拖拽，检查目标位置。
- `ReturnToStartPosition()`：执行归位动画/逻辑。
- `IsValidDropTarget(Vector2 position, out IDropTarget target)`：**[抽象方法]** 检查释放位置是否为有效目标，由子类实现。
- `OnSuccessfulDrop(IDropTarget target)`：**[抽象方法]** 当成功放置在有效目标上时调用，由子类实现。

### MaskCard（UI面具卡牌）
**职责**：继承 DraggableUnit，在 UI 上显示为一张手牌。玩家将其拖到 Unit 身上来更换装备。
**核心方法**：
- `Initialize(Mask maskData)`：绑定逻辑层数据。
- `IsValidDropTarget(...)`：检查是否拖到了友方单位上。
- `OnSuccessfulDrop(IDropTarget target)`：通知 AnimationHandler 请求换装。
- `UpdateVisuals()`：刷新卡牌状态（可用/禁用/高亮）。

### CharacterActionCircle（角色行动圈）
**职责**：继承 DraggableUnit，显示在角色脚下的交互圈。拖拽此圈指向敌人发起攻击。
**核心方法**：
- `IsValidDropTarget(...)`：覆写此方法，检查是否拖到了敌方单位上。
- `OnSuccessfulDrop(IDropTarget target)`：覆写此方法，通知 AnimationHandler 请求攻击（将目标传递给 PlayerController）。

---

## 2. 输入捕捉

### PlayerInput（原始输入处理器）
**职责**：直接对接 Unity Input System / Rewired。捕捉原始硬件信号（鼠标点击、触摸、按键），将其过滤并转化为语义化的信号**传给 AnimationHandler**。
**核心方法**：
- `Enable()` / `Disable()`：启用或禁用输入检测。
- `OnPointerDown(Vector2 position)`：当检测到指针按下时调用。
- `OnPointerUp(Vector2 position)`：当检测到指针抬起时调用。
- `OnDrag(Vector2 delta)`：当检测到拖拽时调用。
- `OnKeyPressed(KeyCode key)`：当检测到按键时调用。
- `(Event) OnCardDragged(MaskCard card, BattleUnit target)`：当卡牌被拖拽到有效目标上时，触发事件并通知 AnimationHandler。
- `(Event) OnActionButtonPressed(ActionType action)`：当行动按钮被按下时，触发事件并通知 AnimationHandler。

---

## 3. 动画处理器

### AnimationHandler（动画处理器）含有音效处理
**职责**：统一处理**合法交互**后的动画播放，并在动画的关键节点向逻辑组件派发指令（伤害、生效、装备等）。**玩家输入只传给 AnimationHandler**，由它在合适的动画时机驱动数据逻辑与表现同步。

**需要处理的合法交互**：
1. **行动圈拖到敌方单位**：播放技能/攻击动画；在命中帧触发伤害指令；动画结束后处理死亡与消除。
2. **MaskCard 拖到友方单位**：播放装上 Mask 的动画；在合适节点执行装备逻辑。

**核心方法（建议）**：
- `HandleAttackDrag(CharacterActionCircle circle, BattleUnit target)`：接收攻击交互，启动攻击动画流程。
- `HandleMaskEquipDrag(MaskCard card, BattleUnit target)`：接收换装交互，启动换装动画流程。
- `PlayAttackAnimation(UnitController attacker, BattleUnit target)`：播放攻击/技能动画。
- `PlayEquipMaskAnimation(BattleUnit target, Mask mask)`：播放换装动画。
- `OnHitFrame(UnitController attacker, BattleUnit target, ActionCommand command)`：命中帧回调，向目标 `BattleUnit` 下发伤害/状态指令。
- `OnEquipFrame(BattleUnit target, Mask mask)`：换装关键帧回调，执行 `Mask.OnEquip` 等逻辑。
- `OnAnimationComplete()`：动画结束回调，统一处理死亡判定、移除与清理。

**原子化技能流程（示例）**：
1. **MoveToTarget**：单位移动到目标面前。
2. **PlayStrike**：播放斩击特效/动作。
3. **ApplyDamage**：造成伤害（同时触发伤害飘字）。
4. **TriggerPassives**：处理可能的被动效果。
5. **ReturnToOrigin**：单位回到原先位置。
6. **ResolveDeath**：处理目标单位死亡与消除。

TA可以另加类与组来使得动画处理器能够优雅地调用你准备好的动画，可以在pr信息里附上以方便其他人负责AnimationHandler的人调用。
---


# III. 扩展定义 (Shared Definitions)
- **ActionCommand**：行动指令数据包（发起者、目标、技能ID）。
- **StatusEffect**：状态效果抽象（持续回合、叠层、触发条件）。
- **ResourceType**：资源的枚举（ActionPoint, Mana, MaskEnergy）。
- **Target**：目标选择器（Single, AllEnemies, Self）。
- **Skill**：技能抽象（冷却、消耗、效果）。
- **SkillStep**：技能步骤原子单元（MoveToTarget, PlayStrike, ApplyDamage, TriggerPassives, ReturnToOrigin, ResolveDeath）。
- **SkillSequence**：技能步骤序列（按顺序编排，供 AnimationHandler 执行）。

---

## 架构依赖总览
1. **输入流**：`PlayerInput` (硬件) -> `DraggableUnit` (交互) -> `AnimationHandler` (动画编排)。
2. **动画驱动逻辑**：`AnimationHandler` (关键帧) -> `PlayerController` / `BattleUnit` / `Mask` (指令与数据变更)。
3. **逻辑流**：`RoundManager` (驱动) -> `UnitController` (决策) -> `BattleUnit` (数据变更)。
4. **表现流**：`BattleUnit` (状态变更) -> (Event/Observer) -> UI Components (更新显示)。
