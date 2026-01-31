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
    [SerializeField] private float destroyDelay = 2f;
    [Tooltip("是否完全销毁GameObject（false=仅禁用）")]
    [SerializeField] private bool destroyGameObject = true;

    [Header("测试配置")]
    [Tooltip("在Unity中配置的玩家单位列表")]
    [SerializeField] private List<BattleUnit> testPlayerUnits = new List<BattleUnit>();
    [Tooltip("在Unity中配置的敌方单位列表")]
    [SerializeField] private List<BattleUnit> testEnemyUnits = new List<BattleUnit>();
    [Tooltip("游戏开始时自动初始化战斗")]
    [SerializeField] private bool autoStartBattle = false;

    // 战斗单位列表（所有参与战斗的单位）
    private List<BattleUnit> battleUnits = new List<BattleUnit>();
    // 待销毁的单位队列
    private List<BattleUnit> unitsToDestroy = new List<BattleUnit>();
    // 当前行动阵营
    private Team currentActiveTeam = Team.Player;
    // 战斗是否正在进行中
    private bool isBattleActive = false;
    // 单位死亡事件处理字典（用于正确取消订阅）
    private Dictionary<BattleUnit, Action> unitDeathHandlers = new Dictionary<BattleUnit, Action>();
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoStartBattle)
        {
            InitializeBattleFromInspector();
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
        // 清理旧的事件订阅
        ClearAllEventSubscriptions();
        
        // 重置战斗状态
        battleUnits.Clear();
        isBattleActive = true;
        currentActiveTeam = Team.Player;

        // 注册玩家单位（强制设为Player阵营）
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.SetTeam(Team.Player);
                RegisterUnit(unit);
            }
        }

        // 注册敌方单位（强制设为Enemy阵营）
        foreach (var unit in enemyUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                unit.SetTeam(Team.Enemy);
                RegisterUnit(unit);
            }
        }

        // 启动第一回合
        StartRound();
    }
    #endregion

    #region 回合流程控制
    /// <summary>开始当前回合（内部驱动）</summary>
    public void StartRound()
    {
        if (!isBattleActive || IsBattleOver()) return;

        // 1. 触发当前阵营所有单位的「回合开始」钩子
        foreach (var unit in GetAllUnitsByTeam(currentActiveTeam))
        {
            if (unit.IsAlive())
            {
                unit.OnTurnStart(); // 单位自身状态更新（如buff生效）
                unit.Controller?.OnTurnStart(); // 控制器逻辑更新（如技能冷却重置）
            }
        }

        // 2. 分配行动点（AP）
        GrantActionPoints();

        // 3. 敌方回合额外逻辑：显示行动预告
        if (currentActiveTeam == Team.Enemy)
        {
            TriggerEnemyActionPreview();
        }

        // 4. 通知显示层：回合开始（更新UI）
        OnRoundStarted?.Invoke(currentActiveTeam);
    }

    /// <summary>结束当前回合（外部调用：如玩家点击结束回合）</summary>
    public void EndRound()
    {
        if (!isBattleActive) return;

        // 1. 触发当前阵营所有单位的「回合结束」钩子
        List<BattleUnit> currentTeamUnits = GetAllUnitsByTeam(currentActiveTeam);
        foreach (var unit in currentTeamUnits)
        {
            if (unit.IsAlive())
            {
                unit.OnTurnEnd(); // 单位自身状态结算（如buff持续回合减少）
                unit.Controller?.OnTurnEnd(); // 控制器逻辑结算（如资源重置）
            }
        }
        
        // 2. 清理死亡单位
        CleanupDeadUnits();

        // 3. 通知显示层：回合结束（更新UI）
        OnRoundEnded?.Invoke(currentActiveTeam);

        // 4. 检查战斗是否结束
        if (IsBattleOver())
        {
            bool isPlayerWin = CheckPlayerVictory();
            OnBattleOver?.Invoke(isPlayerWin);
            isBattleActive = false;
            return;
        }

        // 5. 切换阵营并启动下一轮
        SwapRound();
        StartRound();
    }

    /// <summary>切换行动阵营</summary>
    private void SwapRound()
    {
        Team oldTeam = currentActiveTeam;
        currentActiveTeam = currentActiveTeam == Team.Player ? Team.Enemy : Team.Player;
        OnTeamSwitched?.Invoke(oldTeam, currentActiveTeam);
    }
    #endregion

    #region 资源（AP）管理
    /// <summary>为当前阵营所有单位分配攻击次数，AP恢复为3（恢复至默认值）</summary>
    public void GrantActionPoints()
    {
        PlayerResourceManager.Instance.GainResource(ResourceType.ActionPoint, 3);
        foreach (var unit in GetAllUnitsByTeam(currentActiveTeam))
        {
            if (unit.IsAlive())
            {
                UnitController controller = unit.Controller;
                if (controller != null)
                {
                    controller.GetAttackCount();
                }
            }
        }
        OnActionPointsGranted?.Invoke(currentActiveTeam);
    }

    /// <summary>消耗单位的AP（外部调用：如执行抽卡、佩戴面具）</summary>
    /// <param name="controller">要消耗AP的单位控制器</param>
    /// <param name="amount">消耗的AP数量</param>
    /// <returns>是否消耗成功</returns>
    public bool ConsumeActionPoints(UnitController controller, int amount)
    {
        // 校验条件：控制器有效、单位存活、属于当前行动阵营、AP充足
        if (controller == null
            || !controller.BoundUnit.IsAlive()
            || controller.BoundUnit.UnitTeam != currentActiveTeam
            || !controller.HasResource(ResourceType.ActionPoint, amount))
        {
            Debug.LogWarning($"AP消耗失败：单位{controller.BoundUnit.name}不符合条件");
            return false;
        }

        controller.SpendResource(ResourceType.ActionPoint, amount);
        return true;
    }
    #endregion

    #region 战斗单位管理
    /// <summary>注册战斗单位（添加到战斗列表）</summary>
    public void RegisterUnit(BattleUnit unit)
    {
        if (unit != null && !battleUnits.Contains(unit))
        {
            battleUnits.Add(unit);
            // 监听单位死亡事件，自动移除
            unit.OnDeath += () => UnregisterUnit(unit);
        }
    }

    /// <summary>移除战斗单位（从战斗列表中删除）</summary>
    public void UnregisterUnit(BattleUnit unit)
    {
        if (battleUnits.Remove(unit))
        {
            // 解除死亡事件监听（避免内存泄漏）
            unit.OnDeath -= () => UnregisterUnit(unit);
            // 移除后检查战斗是否结束
            IsBattleOver();
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
        for (int i = battleUnits.Count - 1; i >= 0; i--)
        {
            BattleUnit unit = battleUnits[i];
            if (unit != null && !unit.IsAlive())
            {
                if (!unitsToDestroy.Contains(unit))
                {
                    unitsToDestroy.Add(unit);
                    StartCoroutine(DestroyUnitWithDelay(unit));
                }
            }
        }
    }
    
    /// <summary>延迟销毁单位（用于播放死亡动画）</summary>
    private System.Collections.IEnumerator DestroyUnitWithDelay(BattleUnit unit)
    {
        if (unit == null) yield break;
        
        // 等待死亡动画播放
        yield return new WaitForSeconds(destroyDelay);
        
        // 从待销毁列表中移除
        unitsToDestroy.Remove(unit);
        
        if (unit != null)
        {
            if (destroyGameObject)
            {
                // 完全销毁GameObject
                Destroy(unit.gameObject);
            }
            else
            {
                // 仅禁用GameObject（可能需要保留用于复活或其他机制）
                unit.gameObject.SetActive(false);
            }
            
            Debug.Log($"单位 {unit.gameObject.name} 已被销毁");
        }
    }
    
    /// <summary>立即清理所有死亡单位（战斗结束时调用）</summary>
    public void CleanupAllDeadUnits()
    {
        StopAllCoroutines();
        
        for (int i = battleUnits.Count - 1; i >= 0; i--)
        {
            BattleUnit unit = battleUnits[i];
            if (unit != null && !unit.IsAlive())
            {
                if (destroyGameObject)
                {
                    Destroy(unit.gameObject);
                }
                else
                {
                    unit.gameObject.SetActive(false);
                }
            }
        }
        
        unitsToDestroy.Clear();
    }
    #endregion

    #region 战斗状态判定
    /// <summary>判断战斗是否结束</summary>
    /// <returns>true=战斗结束</returns>
    public bool IsBattleOver()
    {
        bool allPlayersDead = true;
        bool allEnemiesDead = true;

        // 遍历所有单位，检查存活状态
        foreach (var unit in battleUnits)
        {
            if (unit.IsAlive())
            {
                if (unit.UnitTeam == Team.Player)
                {
                    allPlayersDead = false; // 仍有存活的玩家单位
                }
                else if (unit.UnitTeam == Team.Enemy)
                {
                    allEnemiesDead = false; // 仍有存活的敌方单位
                }
            }
        }

        // 战斗结束条件：所有玩家死亡 或 所有敌人死亡
        return allPlayersDead || allEnemiesDead;
    }

    /// <summary>检查玩家是否胜利（仅战斗结束时调用）</summary>
    private bool CheckPlayerVictory()
    {
        // 玩家胜利 = 所有敌人死亡
        foreach (var unit in battleUnits)
        {
            if (unit.UnitTeam == Team.Enemy && unit.IsAlive())
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region 敌方行动预告（衔接显示层）
    /// <summary>触发敌方行动预告（显示层负责渲染UI）</summary>
    private void TriggerEnemyActionPreview()
    {
        foreach (var enemyUnit in GetAllUnitsByTeam(Team.Enemy))
        {
            if (enemyUnit.IsAlive() && enemyUnit.Controller is EnemyController enemyController)
            {
                // 让敌方AI提前决策行动（仅用于预告，不执行）
                ActionCommand previewAction = enemyController.GetPendingAction();
                OnEnemyActionPreview?.Invoke(enemyUnit, previewAction);
            }
        }
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
        if (currentActiveTeam == Team.Player && isBattleActive && !IsBattleOver())
        {
            EndRound();
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
        EndRound();
    }

    [ContextMenu("重置战斗")]
    private void Debug_ResetBattle()
    {
        ClearAllEventSubscriptions();
        isBattleActive = false;
        battleUnits.Clear();
        currentActiveTeam = Team.Player;
    }

    /// <summary>使用Inspector中配置的单位列表初始化战斗</summary>
    public void InitializeBattleFromInspector()
    {
        if (testPlayerUnits.Count == 0 && testEnemyUnits.Count == 0)
        {
            Debug.LogWarning("RoundManager: 未配置任何单位，请在Inspector中设置testPlayerUnits和testEnemyUnits");
            return;
        }
        
        InitializeBattle(testPlayerUnits, testEnemyUnits);
        Debug.Log($"战斗已启动：玩家单位 {testPlayerUnits.Count} 个，敌方单位 {testEnemyUnits.Count} 个");
    }
    #endregion

    #region 事件注册与管理
    /// <summary>订阅所有需要的事件</summary>
    private void SubscribeEvents()
    {
        // 在这里订阅其他系统的事件
        // 例如：如果有全局事件管理器，可以在此订阅
        
        // 单位级别的事件订阅在RegisterUnit中处理
    }

    /// <summary>取消订阅所有事件</summary>
    private void UnsubscribeEvents()
    {
        // 清理所有单位的死亡事件订阅
        foreach (var kvp in unitDeathHandlers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.OnDeath -= kvp.Value;
            }
        }
        unitDeathHandlers.Clear();
        
        // 在这里取消订阅其他系统的事件
    }

    /// <summary>清理所有事件订阅（战斗结束或重置时调用）</summary>
    public void ClearAllEventSubscriptions()
    {
        UnsubscribeEvents();
    }
    #endregion

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
}
