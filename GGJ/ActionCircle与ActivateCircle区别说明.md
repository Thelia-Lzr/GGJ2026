# ActionCircle vs ActivateCircle 区别说明

## 两种圆圈的定位

### ActionCircle（攻击圈/行动圈）
**用途**: 用于拖拽攻击敌人的交互圈

**管理位置**: `UnitController.currentActionCircle`

**创建时机**: 
- 每回合开始时，如果`attackCount > 0`
- 玩家确认攻击后，如果还有剩余攻击次数

**GameObject名称**: "ActionCircle" (来自预制体)

**交互方式**: 
- 鼠标拖拽到敌人身上
- 松开鼠标执行攻击

**生命周期**:
- 回合开始时创建
- 回合结束时销毁（`OnTurnEnd`）
- 使用完攻击次数后自动移除

**实现类**: `ActionCircle.cs`

---

### ActivateCircle（启效果圈/黄圈）
**用途**: 用于右键点击触发面具启效果的交互圈

**管理位置**: `BattleUnit.currentActivateCircle`

**创建时机**:
- 玩家回合开始时，如果面具有启效果（`HasActivateAbility == true`）
- 玩家回合中切换到有启效果的面具时

**预制体**: "ActivateCircle" (从ResourceController加载)

**预制体要求**:
- 必须包含 `ActivateCircle` 组件
- 必须包含 `SpriteRenderer` 组件（设置好Sprite和初始颜色）
- 建议包含 `CircleCollider2D` 组件（设置好半径和isTrigger=true）

**交互方式**:
- 右键点击角色
- 触发面具的`Activate()`方法

**生命周期**:
- 玩家回合开始时创建（如果有启效果）
- 使用启效果后立即销毁
- 玩家回合结束时销毁
- 切换面具时可能被刷新

**实现类**: `ActivateCircle.cs`

**视觉特效**:
- 半透明黄色（由Initialize设置）
- 脉冲动画（透明度0.4-0.6）
- 形状和大小由预制体决定

---

## 两者的独立性

### 关键特性
1. **完全独立**: 两个圆圈使用不同的字段存储，不会互相影响
2. **可以共存**: 同一个单位可以同时拥有ActionCircle和ActivateCircle
3. **不同的父对象管理**: 
   - ActionCircle: 由UnitController管理
   - ActivateCircle: 由BattleUnit管理

### 代码结构
```csharp
// UnitController.cs
private GameObject currentActionCircle;  // 攻击圈

public void InitActionCircle() {
    // 从预制体创建攻击圈用于拖拽攻击
    currentActionCircle = Instantiate(ResourceController.Instance.GetPrefab("ActionCircle"), transform);
}

// BattleUnit.cs
private GameObject currentActivateCircle;  // 黄圈

public void ShowActivateCircle() {
    // 从预制体创建黄圈用于启效果
    GameObject prefab = ResourceController.Instance.GetPrefab("ActivateCircle");
    currentActivateCircle = Instantiate(prefab, transform);
}
```

### 典型场景
一个玩家单位在回合中可能同时存在：
1. **ActionCircle**: 可以拖拽到敌人身上进行攻击
2. **ActivateCircle**: 可以右键点击触发面具启效果

这两个操作完全独立，互不干扰。

---

## 调试识别

### 日志标识
- ActionCircle: `[UnitController] {name} 创建ActionCircle（攻击圈）`
- ActivateCircle: `[BattleUnit] 为 {name} 创建ActivateCircle（启效果黄圈）`

### GameObject命名
- ActionCircle: "ActionCircle" (预制体名)
- ActivateCircle: "ActivateCircle" (预制体名)

### 预制体设置要求

#### ActivateCircle预制体必须包含：
1. **ActivateCircle组件** (脚本)
2. **SpriteRenderer组件**
   - 设置好圆形Sprite
   - 初始颜色会被代码覆盖为黄色
   - SortingOrder会被设置为2
3. **CircleCollider2D组件** (推荐)
   - 设置好半径（如7.0）
   - isTrigger = true
   - 用于右键点击检测

#### ActionCircle预制体必须包含：
1. **ActionCircle组件** (脚本)
2. 其他必要的拖拽交互组件

### 检查方法
在Unity Hierarchy中查看单位对象，应该能看到两个子对象：
```
BattleUnit1
├── ActionCircle(Clone) (如果有攻击次数)
└── ActivateCircle(Clone) (如果面具有启效果)
```

---

## 预制体准备清单

### ActivateCircle预制体设置步骤

1. **创建预制体**
   - 在Resources文件夹中创建预制体
   - 命名为 "ActivateCircle"

2. **添加组件**
   ```
   ActivateCircle (GameObject)
   ├── SpriteRenderer
   │   ├── Sprite: 圆形图片
   │   ├── Color: 任意（会被代码设为黄色）
   │   └── Sorting Order: 任意（会被代码设为2）
   ├── CircleCollider2D
   │   ├── Radius: 7.0 (或根据需要调整)
   │   └── Is Trigger: ?
   └── ActivateCircle (Script)
   ```

3. **注意事项**
   - 预制体的Transform位置会被设为localPosition = (0,0,0)
   - Scale会由预制体本身决定
   - 不需要设置parent，代码会自动设置

4. **ResourceController配置**
   - 确保ResourceController能通过 `GetPrefab("ActivateCircle")` 加载到此预制体

---

## 常见问题

### Q: 为什么我只看到一个圆圈？
A: 可能的原因：
1. 没有攻击次数（attackCount = 0）- 不会显示ActionCircle
2. 面具没有启效果（HasActivateAbility = false）- 不会显示ActivateCircle
3. 已经使用过启效果（CanUseActivate = false）- ActivateCircle被移除

### Q: 两个圆圈会冲突吗？
A: 不会。它们：
- 使用不同的字段存储
- 有不同的GameObject名称
- 由不同的类管理
- 响应不同的输入（拖拽 vs 右键）

### Q: 切换面具后会发生什么？
A: 
- ActionCircle: 不受影响，继续存在
- ActivateCircle: 如果新面具有启效果，会被刷新（先移除旧的，再创建新的）

---

## 性能考虑

### ActionCircle
- 每个单位最多1个
- 在攻击次数耗尽或回合结束时自动清理

### ActivateCircle
- 每个单位最多1个
- 使用后立即销毁
- 由ActivateCircleManager统一管理更新
- 脉冲动画使用协程，性能开销较小

---

## 未来扩展建议

1. **视觉区分**: 给ActionCircle和ActivateCircle不同的颜色
   - ActionCircle: 红色/橙色（攻击）
   - ActivateCircle: 黄色（启效果）

2. **图层分离**: 使用不同的sortingOrder确保不会遮挡

3. **交互提示**: 
   - ActionCircle: 显示攻击次数
   - ActivateCircle: 显示启效果说明

4. **音效**: 为两种交互添加不同的音效
