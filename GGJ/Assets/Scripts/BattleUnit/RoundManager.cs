using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 回合管理器：核心战斗流程驱动中心，负责回合切换、资源分配、战斗状态判定
/// </summary>
/// 
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    #region 配置与状态字段
    [Header("回合配置")]
    [Tooltip("每个单位默认行动点（AP）上限")]
    [SerializeField] private int defaultActionPoints = 3;
    
    [Header("死亡处理配置")]
    [Tooltip("单位死亡后延迟销毁时间（秒），用于播放死亡动画")]
    [SerializeField] private float destroyDelay = 1f;
    [Tooltip("是否完全销毁GameObject（false=仅禁用）")]
    [SerializeField] private bool destroyGameObject = true;

    [Header("测试配置")]
    [Tooltip("测试模式")]
    [SerializeField] private bool isTestMode = false;
    [Tooltip("在Unity中配置的玩家单位列表")]
    [SerializeField] private List<BattleUnit> testPlayerUnits = new List<BattleUnit>();
    [Tooltip("在Unity中配置的敌方单位列表")]
    [SerializeField] private List<BattleUnit> testEnemyUnits = new List<BattleUnit>();
    [Tooltip("游戏开始时自动初始化战斗")]
    [SerializeField] private bool autoStartBattle = false;

    [Header("调试设置")]
    [Tooltip("是否启用详细调试日志")]
    [SerializeField] private bool enableDebugLogs = true;
    [Tooltip("是否在每个回合开始时自动打印状态")]
    [SerializeField] private bool autoPrintRoundStatus = true;
    [Tooltip("是否记录事件触发")]
    [SerializeField] private bool logEvents = true;

    // 战斗单位列表（所有参与战斗的单位）
    public List<BattleUnit> battleUnits = new List<BattleUnit>();
    // 待销毁的单位队列
    private List<BattleUnit> unitsToDestroy = new List<BattleUnit>();
    // 当前行动阵营
    private Team currentActiveTeam = Team.Player;
    // 战斗是否正在进行中
    private bool isBattleActive = false;
    // 单位死亡事件处理字典（用于正确取消订阅）
    private Dictionary<BattleUnit, Action> unitDeathHandlers = new Dictionary<BattleUnit, Action>();
    
    // 回合计数器（用于调试）
    private int roundCounter = 0;
    private int playerRoundCounter = 0;
    private int enemyRoundCounter = 0;
    
    // 面具系统
    private MaskDeck maskDeck;
    [Header("面具系统配置")]
    [Tooltip("每回合抽取的面具数量")]
    [SerializeField] private int masksPerTurn = 1;
    [Tooltip("初始手牌数量")]
    [SerializeField] private int initialHandSize = 3;
    #endregion

    #region 事件定义（衔接显示层）
    /// <summary>回合开始事件（参数：当前行动阵营）</summary>
    public event Action<Team> OnRoundStarted;
    /// <summary>回合结束事件（参数：当前行动阵营）</summary>
    public event Action<Team> OnRoundEnded;
    /// <summary>阵营切换事件（参数：旧阵营，新阵营）</summary>
    public event Action<Team, Team> OnTeamSwitched;
    /// <summary>AP分配完成事件（参数：当前行动阵营）</summary>
    public event Action<Team> OnActionPointsGranted;
    /// <summary>敌人行动预告事件（参数：敌人单位，预告行动）</summary>
    public event Action<BattleUnit, ActionCommand> OnEnemyActionPreview;
    /// <summary>战斗结束事件（参数：是否玩家胜利）</summary>
    public event Action<bool> OnBattleOver;
    #endregion
    #region 单例初始化
    private void Awake()
    {
        DebugLog("═══════════════════════════════════════════════");
        DebugLog("[RoundManager] Awake() START");
        
        if (Instance != null && Instance != this)
        {
            DebugLog($"[RoundManager] ⚠️ 检测到重复实例，销毁 {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        DebugLog($"[RoundManager] ✅ 单例初始化完成");
        DebugLog($"  配置: AP={defaultActionPoints}, DestroyDelay={destroyDelay}s");
        DebugLog($"  AutoStart={autoStartBattle}, TestMode={isTestMode}");
        DebugLog("═══════════════════════════════════════════════");
    }

    private void Start()
    {
        DebugLog("[RoundManager] Start() - 检查自动启动战斗");
        
        if (autoStartBattle)
        {
            DebugLog("[RoundManager] ✅ autoStartBattle=true, 调用 InitializeBattleFromInspector()");
            InitializeBattleFromInspector();
        }
        else
        {
            DebugLog("[RoundManager] ⏸️ autoStartBattle=false, 等待手动启动");
        }
    }
    #endregion

    #region 核心战斗初始化
    /// <summary>
    /// 初始化战斗（外部调用：如战斗开始时）
    /// </summary>
    /// <param name="playerUnits">玩家方单位列表</param>
    /// <param name="enemyUnits">敌方单位列表</param>
    public void InitializeBattle(List<BattleUnit> playerUnits, List<BattleUnit> enemyUnits)
    {
        DebugLog("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DebugLog("[RoundManager] InitializeBattle() START");
        DebugLog($"  输入参数: 玩家单位={playerUnits?.Count ?? 0}, 敌方单位={enemyUnits?.Count ?? 0}");
        
        // 清理旧的事件订阅
        DebugLog("[RoundManager] 步骤1: 清理旧事件订阅");
        ClearAllEventSubscriptions();
        
        // 重置战斗状态
        DebugLog("[RoundManager] 步骤2: 重置战斗状态");
        battleUnits.Clear();
        isBattleActive = true;
        currentActiveTeam = Team.Player;
        roundCounter = 0;
        playerRoundCounter = 0;
        enemyRoundCounter = 0;
        DebugLog($"  battleUnits.Clear() 完成");
        DebugLog($"  isBattleActive = {isBattleActive}");
        DebugLog($"  currentActiveTeam = {currentActiveTeam}");

        // 注册玩家单位（强制设为Player阵营）
        DebugLog("[RoundManager] 步骤3: 注册玩家单位");
        int playerRegistered = 0;
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.SetTeam(Team.Player);
                RegisterUnit(unit);
                playerRegistered++;
                DebugLog($"  ✅ 玩家单位注册: {unit.gameObject.name} (HP: {unit.CurrentHealth}/{unit.MaxHealth})");
            }
            else
            {
                DebugLog($"  ⚠️ 跳过无效玩家单位: {(unit != null ? unit.gameObject.name + " (已死亡)" : "NULL")}");
            }
        }
        DebugLog($"  玩家单位注册完成: {playerRegistered}/{playerUnits.Count}");

        // 注册敌方单位（强制设为Enemy阵营）
        DebugLog("[RoundManager] 步骤4: 注册敌方单位");
        int enemyRegistered = 0;
        foreach (var unit in enemyUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.SetTeam(Team.Enemy);
                RegisterUnit(unit);
                enemyRegistered++;
                DebugLog($"  ✅ 敌方单位注册: {unit.gameObject.name} (HP: {unit.CurrentHealth}/{unit.MaxHealth})");
            }
            else
            {
                DebugLog($"  ⚠️ 跳过无效敌方单位: {(unit != null ? unit.gameObject.name + " (已死亡)" : "NULL")}");
            }
        }
        DebugLog($"  敌方单位注册完成: {enemyRegistered}/{enemyUnits.Count}");

        DebugLog($"[RoundManager] 总单位数: {battleUnits.Count} (玩家: {playerRegistered}, 敌人: {enemyRegistered})");
        
        // 初始化面具系统
        DebugLog("[RoundManager] 步骤4.5: 初始化面具系统");
        InitializeMaskSystem();
        
        // 启动第一回合
        DebugLog("[RoundManager] 步骤5: 启动第一回合");
        StartRound();
        
        DebugLog("[RoundManager] InitializeBattle() COMPLETE ✅");
        DebugLog("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
    #endregion

    #region 回合流程控制
    /// <summary>开始当前回合（内部驱动）</summary>
    public void StartRound()
    {
        roundCounter++;
        if (currentActiveTeam == Team.Player)
            playerRoundCounter++;
        else
            enemyRoundCounter++;
        
        DebugLog("╔═══════════════════════════════════════════════╗");
        DebugLog($"║  回合 #{roundCounter} 开始 - {currentActiveTeam}");
        DebugLog($"║  (玩家回合: {playerRoundCounter}, 敌方回合: {enemyRoundCounter})");
        DebugLog("╚═══════════════════════════════════════════════╝");
        
        if (!isBattleActive)
        {
            Debug.LogWarning($"[RoundManager] ⚠️ StartRound() ABORTED - isBattleActive=false");
            return;
        }
        
        if (IsBattleOver())
        {
            Debug.LogWarning($"[RoundManager] ⚠️ StartRound() ABORTED - 战斗已结束");
            return;
        }

        // 1. 触发当前阵营所有单位的「回合开始」钩子
        DebugLog($"[RoundManager] 步骤1: 触发 {currentActiveTeam} 阵营单位的回合开始钩子");
        List<BattleUnit> activeTeamUnits = GetAllUnitsByTeam(currentActiveTeam);
        DebugLog($"  当前阵营活跃单位数: {activeTeamUnits.Count}");
        
        foreach (var unit in activeTeamUnits)
        {
            if (unit.IsAlive())
            {
                DebugLog($"  → 调用 {unit.gameObject.name}.OnTurnStart()");
                unit.OnTurnStart(); // 单位自身状态更新（如buff生效）
                
                if (unit.Controller != null)
                {
                    DebugLog($"  → 调用 {unit.gameObject.name}.Controller.OnTurnStart()");
                    unit.Controller.OnTurnStart(); // 控制器逻辑更新（如技能冷却重置）
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {unit.gameObject.name} 的 Controller 为空！");
                }
            }
        }

        // 2. 分配行动点（AP）
        DebugLog($"[RoundManager] 步骤2: 分配行动点");
        GrantActionPoints();
        
        // 2.5 玩家回合抽牌
        if (currentActiveTeam == Team.Player)
        {
            DebugLog($"[RoundManager] 步骤2.5: 玩家抽取面具牌");
            PlayerResourceManager.Instance.DrawMasks(masksPerTurn);
        }

        // 3. 敌方回合额外逻辑：显示行动预告
        if (currentActiveTeam == Team.Enemy)
        {
            DebugLog($"[RoundManager] 步骤3: 触发敌方行动预告");
            TriggerEnemyActionPreview();
        }
        else
        {
            DebugLog($"[RoundManager] 步骤3: 跳过（玩家回合无需预告）");
        }

        // 4. 通知显示层：回合开始（更新UI）
        DebugLog($"[RoundManager] 步骤4: 触发 OnRoundStarted 事件");
        LogEvent($"OnRoundStarted({currentActiveTeam})");
        OnRoundStarted?.Invoke(currentActiveTeam);
        
        if (autoPrintRoundStatus)
        {
            PrintBattleStatus();
        }
        
        DebugLog($"[RoundManager] StartRound() COMPLETE ✅");
    }

    /// <summary>结束当前回合（外部调用：如玩家点击结束回合）</summary>
    public void EndRound()
    {
        DebugLog("┌───────────────────────────────────────────────┐");
        DebugLog($"│  回合 #{roundCounter} 结束 - {currentActiveTeam}");
        DebugLog("└───────────────────────────────────────────────┘");
        
        if (!isBattleActive)
        {
            Debug.LogWarning($"[RoundManager] ⚠️ EndRound() ABORTED - isBattleActive=false");
            return;
        }

        // 1. 触发当前阵营所有单位的「回合结束」钩子
        DebugLog($"[RoundManager] 步骤1: 触发 {currentActiveTeam} 阵营单位的回合结束钩子");
        List<BattleUnit> currentTeamUnits = GetAllUnitsByTeam(currentActiveTeam);
        DebugLog($"  当前阵营活跃单位数: {currentTeamUnits.Count}");
        
        foreach (var unit in currentTeamUnits)
        {
            if (unit.IsAlive())
            {
                DebugLog($"  → 调用 {unit.gameObject.name}.OnTurnEnd()");
                unit.OnTurnEnd(); // 单位自身状态结算（如buff持续回合减少）
                
                if (unit.Controller != null)
                {
                    DebugLog($"  → 调用 {unit.gameObject.name}.Controller.OnTurnEnd()");
                    unit.Controller.OnTurnEnd(); // 控制器逻辑结算（如资源重置）
                }
            }
        }
        
        // 2. 清理死亡单位
        DebugLog($"[RoundManager] 步骤2: 清理死亡单位");
        CleanupDeadUnits();

        // 3. 通知显示层：回合结束（更新UI）
        DebugLog($"[RoundManager] 步骤3: 触发 OnRoundEnded 事件");
        LogEvent($"OnRoundEnded({currentActiveTeam})");
        OnRoundEnded?.Invoke(currentActiveTeam);

        // 4. 检查战斗是否结束
        DebugLog($"[RoundManager] 步骤4: 检查战斗是否结束");
        if (IsBattleOver())
        {
            bool isPlayerWin = CheckPlayerVictory();
            DebugLog($"[RoundManager] ⚔️ 战斗结束！玩家{(isPlayerWin ? "胜利" : "失败")} 🎮");
            LogEvent($"OnBattleOver(isPlayerWin={isPlayerWin})");
            OnBattleOver?.Invoke(isPlayerWin);
            isBattleActive = false;
            
            DebugLog($"[RoundManager] EndRound() COMPLETE - 战斗已结束 ✅");
            return;
        }

        // 5. 切换阵营并启动下一轮
        DebugLog($"[RoundManager] 步骤5: 切换阵营并启动下一回合");
        SwapRound();
        StartRound();
        
        DebugLog($"[RoundManager] EndRound() COMPLETE ✅");
    }

    /// <summary>切换行动阵营</summary>
    private void SwapRound()
    {
        Team oldTeam = currentActiveTeam;
        currentActiveTeam = currentActiveTeam == Team.Player ? Team.Enemy : Team.Player;
        
        DebugLog($"[RoundManager] SwapRound() - 阵营切换: {oldTeam} → {currentActiveTeam}");
        LogEvent($"OnTeamSwitched({oldTeam}, {currentActiveTeam})");
        OnTeamSwitched?.Invoke(oldTeam, currentActiveTeam);
    }
    #endregion

    #region 资源（AP）管理
    /// <summary>为当前阵营所有单位分配攻击次数，AP恢复为3（恢复至默认值）</summary>
    public void GrantActionPoints()
    {
        DebugLog($"[RoundManager] GrantActionPoints() START - Team: {currentActiveTeam}");
        
        // 恢复资源池
        DebugLog($"  → PlayerResourceManager.GainResource(ActionPoint, {defaultActionPoints})");
        if (currentActiveTeam == Team.Player)
        {
            PlayerResourceManager.Instance.GainResource(ResourceType.ActionPoint, defaultActionPoints);
        }
        

            // 为每个单位分配攻击次数
        List<BattleUnit> teamUnits = GetAllUnitsByTeam(currentActiveTeam);
        DebugLog($"  分配目标: {teamUnits.Count} 个单位");
        
        foreach (var unit in teamUnits)
        {
            if (unit.IsAlive())
            {
                UnitController controller = unit.Controller;
                if (controller != null)
                {
                    DebugLog($"  → {unit.gameObject.name}.Controller.GetAttackCount()");
                    controller.GetAttackCount();
                    if (controller.attackCount > 0)
                    {
                        controller.InitActionCircle();
                    }
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {unit.gameObject.name} 的 Controller 为空，跳过 AP 分配");
                }
            }
        }
        
        LogEvent($"OnActionPointsGranted({currentActiveTeam})");
        OnActionPointsGranted?.Invoke(currentActiveTeam);
        
        DebugLog($"[RoundManager] GrantActionPoints() COMPLETE ✅");
    }

    /// <summary>消耗单位的AP（外部调用：如执行抽卡、佩戴面具）</summary>
    /// <param name="controller">要消耗AP的单位控制器</param>
    /// <param name="amount">消耗的AP数量</param>
    /// <returns>是否消耗成功</returns>
    public bool ConsumeActionPoints(UnitController controller, int amount)
    {
        DebugLog($"[RoundManager] ConsumeActionPoints() - Amount: {amount}");
        
        if (controller == null)
        {
            Debug.LogWarning($"[RoundManager] ⚠️ ConsumeActionPoints() FAILED - Controller 为空");
            return false;
        }
        
        DebugLog($"  Controller: {controller.GetType().Name}");
        DebugLog($"  Unit: {controller.BoundUnit.gameObject.name}");
        
        // 校验条件：控制器有效、单位存活、属于当前行动阵营、AP充足
        if (!controller.BoundUnit.IsAlive())
        {
            Debug.LogWarning($"[RoundManager] ⚠️ ConsumeActionPoints() FAILED - 单位已死亡");
            return false;
        }
        
        if (controller.BoundUnit.UnitTeam != currentActiveTeam)
        {
            Debug.LogWarning($"[RoundManager] ⚠️ ConsumeActionPoints() FAILED - 不是当前行动阵营 (单位:{controller.BoundUnit.UnitTeam}, 当前:{currentActiveTeam})");
            return false;
        }
        
        if (!controller.HasResource(ResourceType.ActionPoint, amount))
        {
            Debug.LogWarning($"[RoundManager] ⚠️ ConsumeActionPoints() FAILED - AP 不足");
            return false;
        }

        DebugLog($"  ✅ 验证通过，消耗 {amount} AP");
        controller.SpendResource(ResourceType.ActionPoint, amount);
        
        DebugLog($"[RoundManager] ConsumeActionPoints() SUCCESS ✅");
        return true;
    }
    #endregion

    #region 战斗单位管理
    /// <summary>注册战斗单位（添加到战斗列表）</summary>
    public void RegisterUnit(BattleUnit unit)
    {
        DebugLog($"[RoundManager] RegisterUnit() - Unit: {(unit != null ? unit.gameObject.name : "NULL")}");
        
        if (unit == null)
        {
            Debug.LogWarning($"[RoundManager] ⚠️ RegisterUnit() ABORTED - Unit 为空");
            return;
        }
        
        if (battleUnits.Contains(unit))
        {
            Debug.LogWarning($"[RoundManager] ⚠️ RegisterUnit() ABORTED - {unit.gameObject.name} 已在列表中");
            return;
        }
        
        battleUnits.Add(unit);
        DebugLog($"  ✅ 添加到 battleUnits 列表 (新总数: {battleUnits.Count})");
        
        // 创建并存储委托引用
        Action handler = () => UnregisterUnit(unit);
        unitDeathHandlers[unit] = handler;
        unit.OnDeath += handler;
        DebugLog($"  ✅ 订阅 OnDeath 事件");
        
        DebugLog($"[RoundManager] RegisterUnit() COMPLETE - {unit.gameObject.name} 注册成功 ✅");
    }

    /// <summary>移除战斗单位（从战斗列表中删除）</summary>
    public void UnregisterUnit(BattleUnit unit)
    {
        DebugLog($"[RoundManager] UnregisterUnit() - Unit: {(unit != null ? unit.gameObject.name : "NULL")}");
        
        if (battleUnits.Remove(unit))
        {
            DebugLog($"  ✅ 从 battleUnits 移除 (剩余: {battleUnits.Count})");
            
            // 使用存储的委托引用取消订阅
            if (unitDeathHandlers.TryGetValue(unit, out Action handler))
            {
                unit.OnDeath -= handler;
                unitDeathHandlers.Remove(unit);
                DebugLog($"  ✅ 取消 OnDeath 事件订阅");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ 未找到 OnDeath 处理器");
            }
            
            DebugLog($"  → 检查战斗是否结束");
            IsBattleOver();
            
            DebugLog($"[RoundManager] UnregisterUnit() COMPLETE ✅");
        }
        else
        {
            Debug.LogWarning($"[RoundManager] ⚠️ UnregisterUnit() - 单位不在列表中");
        }
    }

    /// <summary>根据阵营获取所有单位</summary>
    private List<BattleUnit> GetAllUnitsByTeam(Team team)
    {
        List<BattleUnit> result = new List<BattleUnit>();
        foreach (var unit in battleUnits)
        {
            if (unit.UnitTeam == team && unit.IsAlive())
            {
                result.Add(unit);
            }
        }
        return result;
    }
    
    /// <summary>清理死亡单位（标记待销毁）</summary>
    private void CleanupDeadUnits()
    {
        DebugLog($"[RoundManager] CleanupDeadUnits() START - 检查 {battleUnits.Count} 个单位");
        
        int deadCount = 0;
        for (int i = battleUnits.Count - 1; i >= 0; i--)
        {
            BattleUnit unit = battleUnits[i];
            if (unit != null && !unit.IsAlive())
            {
                if (!unitsToDestroy.Contains(unit))
                {
                    deadCount++;
                    DebugLog($"  💀 发现死亡单位: {unit.gameObject.name}");
                    unitsToDestroy.Add(unit);
                    DebugLog($"  → 启动延迟销毁协程 (延迟: {destroyDelay}s)");
                    StartCoroutine(DestroyUnitWithDelay(unit));
                }
            }
        }
        
        DebugLog($"[RoundManager] CleanupDeadUnits() COMPLETE - 处理了 {deadCount} 个死亡单位");
    }
    
    /// <summary>延迟销毁单位（用于播放死亡动画）</summary>
    private System.Collections.IEnumerator DestroyUnitWithDelay(BattleUnit unit)
    {
        if (unit == null)
        {
            DebugLog($"[RoundManager] DestroyUnitWithDelay() - Unit 为空，中止协程");
            yield break;
        }
        
        string unitName = unit.gameObject.name;
        DebugLog($"[RoundManager] DestroyUnitWithDelay() - 等待 {destroyDelay}s: {unitName}");
        
        // 等待死亡动画播放
        yield return new WaitForSeconds(destroyDelay);
        
        // 从待销毁列表中移除
        unitsToDestroy.Remove(unit);
        
        if (unit != null)
        {
            if (destroyGameObject)
            {
                DebugLog($"[RoundManager] 🗑️ 完全销毁GameObject: {unitName}");
                Destroy(unit.gameObject);
            }
            else
            {
                DebugLog($"[RoundManager] 🔒 禁用GameObject: {unitName}");
                unit.gameObject.SetActive(false);
            }
        }
        else
        {
            DebugLog($"[RoundManager] ⚠️ {unitName} 已被提前销毁");
        }
    }
    
    /// <summary>立即清理所有死亡单位（战斗结束时调用）</summary>
    public void CleanupAllDeadUnits()
    {
        DebugLog($"[RoundManager] CleanupAllDeadUnits() - 立即清理所有死亡单位");
        DebugLog($"  停止所有协程");
        StopAllCoroutines();
        
        int cleanedCount = 0;
        for (int i = battleUnits.Count - 1; i >= 0; i--)
        {
            BattleUnit unit = battleUnits[i];
            if (unit != null && !unit.IsAlive())
            {
                cleanedCount++;
                string unitName = unit.gameObject.name;
                
                if (destroyGameObject)
                {
                    DebugLog($"  🗑️ 销毁: {unitName}");
                    Destroy(unit.gameObject);
                }
                else
                {
                    DebugLog($"  🔒 禁用: {unitName}");
                    unit.gameObject.SetActive(false);
                }
            }
        }
        
        unitsToDestroy.Clear();
        DebugLog($"[RoundManager] CleanupAllDeadUnits() COMPLETE - 清理了 {cleanedCount} 个单位");
    }
    #endregion

    #region 战斗状态判定
    /// <summary>判断战斗是否结束</summary>
    /// <returns>true=战斗结束</returns>
    public bool IsBattleOver()
    {
        bool allPlayersDead = true;
        bool allEnemiesDead = true;
        
        int alivePlayerCount = 0;
        int aliveEnemyCount = 0;

        // 遍历所有单位，检查存活状态
        foreach (var unit in battleUnits)
        {
            if (unit.IsAlive())
            {
                if (unit.UnitTeam == Team.Player)
                {
                    allPlayersDead = false;
                    alivePlayerCount++;
                }
                else if (unit.UnitTeam == Team.Enemy)
                {
                    allEnemiesDead = false;
                    aliveEnemyCount++;
                }
            }
        }

        bool isBattleOver = allPlayersDead || allEnemiesDead;
        
        if (isBattleOver)
        {
            DebugLog($"[RoundManager] IsBattleOver() = TRUE ⚠️");
            DebugLog($"  存活玩家: {alivePlayerCount}, 存活敌人: {aliveEnemyCount}");
        }

        return isBattleOver;
    }

    /// <summary>检查玩家是否胜利（仅战斗结束时调用）</summary>
    private bool CheckPlayerVictory()
    {
        DebugLog($"[RoundManager] CheckPlayerVictory() - 检查胜利条件");
        
        // 玩家胜利 = 所有敌人死亡
        foreach (var unit in battleUnits)
        {
            if (unit.UnitTeam == Team.Enemy && unit.IsAlive())
            {
                DebugLog($"  ❌ 仍有敌人存活: {unit.gameObject.name}");
                return false;
            }
        }
        
        DebugLog($"  ✅ 所有敌人已死亡 - 玩家胜利！");
        return true;
    }
    #endregion

    #region 面具系统集成
    /// <summary>初始化面具系统（创建牌组并发初始手牌）</summary>
    private void InitializeMaskSystem()
    {
        DebugLog("[RoundManager] InitializeMaskSystem() START");
        
        // 使用 MaskFactory 创建标准牌组
        List<Mask> initialDeck = MaskFactory.CreateStandardDeck();
        DebugLog($"  创建牌组: {initialDeck.Count} 张面具");
        
        // 初始化 PlayerResourceManager 的牌组
        PlayerResourceManager.Instance.InitializeDeck(initialDeck);
        
        // 发初始手牌
        DebugLog($"  发初始手牌: {initialHandSize} 张");
        PlayerResourceManager.Instance.DrawMasks(initialHandSize);
        
        DebugLog("[RoundManager] InitializeMaskSystem() COMPLETE ✅");
    }
    #endregion

    #region 敌方行动预告（衔接显示层）
    /// <summary>触发敌方行动预告（显示层负责渲染UI）</summary>
    private void TriggerEnemyActionPreview()
    {
        DebugLog($"[RoundManager] TriggerEnemyActionPreview() - 生成敌方行动预告");
        
        List<BattleUnit> enemies = GetAllUnitsByTeam(Team.Enemy);
        DebugLog($"  敌方单位数: {enemies.Count}");
        
        int previewCount = 0;
        foreach (var enemyUnit in enemies)
        {
            if (enemyUnit.IsAlive() && enemyUnit.Controller is EnemyController enemyController)
            {
                // 让敌方AI提前决策行动（仅用于预告，不执行）
                ActionCommand previewAction = enemyController.GetPendingAction();
                
                if (previewAction != null)
                {
                    previewCount++;
                    DebugLog($"  → {enemyUnit.gameObject.name}: {previewAction.ActionType} -> {(previewAction.Target != null ? previewAction.Target.gameObject.name : "NULL")}");
                    LogEvent($"OnEnemyActionPreview({enemyUnit.gameObject.name}, {previewAction.ActionType})");
                    OnEnemyActionPreview?.Invoke(enemyUnit, previewAction);
                }
                else
                {
                    DebugLog($"  ⚠️ {enemyUnit.gameObject.name}: 无预告行动");
                }
            }
        }
        
        DebugLog($"[RoundManager] TriggerEnemyActionPreview() COMPLETE - 生成 {previewCount} 个预告");
    }
    #endregion

    #region 外部访问接口
    /// <summary>获取当前行动阵营</summary>
    public Team GetActiveTeam() => currentActiveTeam;

    /// <summary>获取战斗是否正在进行</summary>
    public bool IsBattleActive() => isBattleActive;

    /// <summary>玩家主动结束回合（外部UI调用）</summary>
    public void PlayerEndTurn()
    {
        DebugLog($"[RoundManager] PlayerEndTurn() - 玩家请求结束回合");
        DebugLog($"  当前阵营: {currentActiveTeam}, 战斗中: {isBattleActive}, 战斗结束: {IsBattleOver()}");
        
        if (currentActiveTeam == Team.Player && isBattleActive && !IsBattleOver())
        {
            DebugLog($"  ✅ 条件满足，调用 EndRound()");
            EndRound();
        }
        else
        {
            Debug.LogWarning($"[RoundManager] ⚠️ PlayerEndTurn() 条件不满足");
            Debug.LogWarning($"  currentActiveTeam={currentActiveTeam}, isBattleActive={isBattleActive}, IsBattleOver={IsBattleOver()}");
        }
    }
    #endregion

    #region 编辑器调试（可选）
    [ContextMenu("使用Inspector配置启动战斗")]
    private void Debug_InitializeBattleFromInspector()
    {
        InitializeBattleFromInspector();
    }

    [ContextMenu("强制结束当前回合")]
    private void Debug_EndCurrentRound()
    {
        DebugLog($"[RoundManager] 🔧 手动触发: 强制结束当前回合");
        EndRound();
    }

    [ContextMenu("重置战斗")]
    private void Debug_ResetBattle()
    {
        DebugLog($"[RoundManager] 🔧 手动触发: 重置战斗");
        ClearAllEventSubscriptions();
        isBattleActive = false;
        battleUnits.Clear();
        currentActiveTeam = Team.Player;
        roundCounter = 0;
        playerRoundCounter = 0;
        enemyRoundCounter = 0;
        DebugLog($"[RoundManager] 战斗已重置 ✅");
    }

    [ContextMenu("打印战斗状态")]
    private void Debug_PrintBattleStatus()
    {
        PrintBattleStatus();
    }

    [ContextMenu("打印所有事件订阅")]
    private void Debug_PrintEventSubscriptions()
    {
        Debug.Log("═════════════════════════════════════════════════");
        Debug.Log("[RoundManager] 事件订阅统计");
        Debug.Log($"  OnRoundStarted: {GetSubscriberCount(OnRoundStarted)}");
        Debug.Log($"  OnRoundEnded: {GetSubscriberCount(OnRoundEnded)}");
        Debug.Log($"  OnTeamSwitched: {GetSubscriberCount(OnTeamSwitched)}");
        Debug.Log($"  OnActionPointsGranted: {GetSubscriberCount(OnActionPointsGranted)}");
        Debug.Log($"  OnEnemyActionPreview: {GetSubscriberCount(OnEnemyActionPreview)}");
        Debug.Log($"  OnBattleOver: {GetSubscriberCount(OnBattleOver)}");
        Debug.Log($"  单位死亡处理器: {unitDeathHandlers.Count}");
        Debug.Log("═════════════════════════════════════════════════");
    }

    /// <summary>使用Inspector中配置的单位列表初始化战斗</summary>
    public void InitializeBattleFromInspector()
    {
        DebugLog($"[RoundManager] InitializeBattleFromInspector() - 使用 Inspector 配置");
        
        if (testPlayerUnits.Count == 0 && testEnemyUnits.Count == 0)
        {
            Debug.LogWarning("RoundManager: ⚠️ 未配置任何单位，请在Inspector中设置testPlayerUnits和testEnemyUnits");
            return;
        }
        
        DebugLog($"  testPlayerUnits: {testPlayerUnits.Count}");
        DebugLog($"  testEnemyUnits: {testEnemyUnits.Count}");
        
        InitializeBattle(testPlayerUnits, testEnemyUnits);
    }
    #endregion

    #region 事件注册与管理
    /// <summary>订阅所有需要的事件</summary>
    private void SubscribeEvents()
    {
        DebugLog($"[RoundManager] SubscribeEvents() - 订阅系统事件");
        // 在这里订阅其他系统的事件
        // 例如：如果有全局事件管理器，可以在此订阅
        
        // 单位级别的事件订阅在RegisterUnit中处理
    }

    /// <summary>取消订阅所有事件</summary>
    private void UnsubscribeEvents()
    {
        DebugLog($"[RoundManager] UnsubscribeEvents() - 取消所有事件订阅");
        
        // 清理所有单位的死亡事件订阅
        int unsubscribedCount = 0;
        foreach (var kvp in unitDeathHandlers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.OnDeath -= kvp.Value;
                unsubscribedCount++;
            }
        }
        unitDeathHandlers.Clear();
        
        DebugLog($"  ✅ 取消了 {unsubscribedCount} 个单位死亡事件订阅");
    }

    /// <summary>清理所有事件订阅（战斗结束或重置时调用）</summary>
    public void ClearAllEventSubscriptions()
    {
        DebugLog($"[RoundManager] ClearAllEventSubscriptions()");
        UnsubscribeEvents();
    }
    #endregion

    private void OnEnable()
    {
        DebugLog($"[RoundManager] OnEnable()");
        SubscribeEvents();
    }

    private void OnDisable()
    {
        DebugLog($"[RoundManager] OnDisable()");
        UnsubscribeEvents();
    }

    #region Debug Helper Methods
    /// <summary>统一的调试日志方法</summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    /// <summary>记录事件触发</summary>
    private void LogEvent(string eventInfo)
    {
        if (logEvents && enableDebugLogs)
        {
            Debug.Log($"[RoundManager] 🎯 事件触发: {eventInfo}");
        }
    }
    
    /// <summary>打印完整战斗状态</summary>
    private void PrintBattleStatus()
    {
        Debug.Log("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        Debug.Log($"┃  战斗状态报告 - 回合 #{roundCounter}");
        Debug.Log("┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫");
        Debug.Log($"┃  战斗状态: {(isBattleActive ? "进行中 🔥" : "未开始/已结束")}");
        Debug.Log($"┃  当前阵营: {currentActiveTeam}");
        Debug.Log($"┃  回合统计: 总计 {roundCounter} | 玩家 {playerRoundCounter} | 敌人 {enemyRoundCounter}");
        Debug.Log($"┃  战斗单位总数: {battleUnits.Count}");
        Debug.Log($"┃  待销毁单位: {unitsToDestroy.Count}");
        Debug.Log("┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫");
        
        // 玩家单位
        List<BattleUnit> playerUnits = GetAllUnitsByTeam(Team.Player);
        Debug.Log($"┃  🛡️  玩家单位 ({playerUnits.Count}):");
        foreach (var unit in playerUnits)
        {
            Debug.Log($"┃    • {unit.gameObject.name} - HP: {unit.CurrentHealth}/{unit.MaxHealth}");
        }
        
        // 敌方单位
        List<BattleUnit> enemyUnits = GetAllUnitsByTeam(Team.Enemy);
        Debug.Log($"┃  ⚔️  敌方单位 ({enemyUnits.Count}):");
        foreach (var unit in enemyUnits)
        {
            Debug.Log($"┃    • {unit.gameObject.name} - HP: {unit.CurrentHealth}/{unit.MaxHealth}");
        }
        
        Debug.Log("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
    }
    
    /// <summary>获取事件订阅者数量</summary>
    private int GetSubscriberCount(Delegate eventDelegate)
    {
        return eventDelegate?.GetInvocationList().Length ?? 0;
    }
    #endregion
}
