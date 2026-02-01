using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

/// <summary>
/// 战斗单位配置
/// </summary>
[Serializable]
public class UnitConfig
{
    [Tooltip("Controller类型名称（如：PlayerController, EnemyTank）")]
    public string controllerTypeName;
    
    public UnitConfig() { }
    
    public UnitConfig(string typeName)
    {
        controllerTypeName = typeName;
    }
}

/// <summary>
/// 战斗生成器：负责生成战斗场景、管理单位入场和敌人替补
/// 通过配置Controller类型名称，自动创建完整的战斗单位GameObject
/// </summary>
public class BattleGenerator : MonoBehaviour
{
    public static BattleGenerator Instance { get; private set; }
    
    [Header("启动配置")]
    [Tooltip("是否在Start时自动启动战斗")]
    [SerializeField] private bool autoStartBattle = true;
    
    [Header("单位配置（填写Controller类型名称）")]
    [Tooltip("友方单位配置（必须有3个）")]
    [SerializeField] private List<UnitConfig> playerConfigs = new List<UnitConfig>();
    
    [Tooltip("敌方单位配置（前3个会首先出场，其余作为替补）")]
    [SerializeField] private List<UnitConfig> enemyConfigs = new List<UnitConfig>();
    
    [Header("单位生成位置")]
    [SerializeField] private Vector2 playerTopPosition = new Vector2(-5f, 2f);
    [SerializeField] private Vector2 playerMiddlePosition = new Vector2(-5f, 0f);
    [SerializeField] private Vector2 playerBottomPosition = new Vector2(-5f, -2f);
    
    [SerializeField] private Vector2 enemyTopPosition = new Vector2(5f, 2f);
    [SerializeField] private Vector2 enemyMiddlePosition = new Vector2(5f, 0f);
    [SerializeField] private Vector2 enemyBottomPosition = new Vector2(5f, -2f);
    
    [Header("动画配置")]
    [SerializeField] private float offscreenOffset = 10f;
    [SerializeField] private float entranceDuration = 1f;
    
    [Header("调试")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 运行时数据
    private List<BattleUnit> activePlayerUnits = new List<BattleUnit>();
    private List<BattleUnit> activeEnemyUnits = new List<BattleUnit>();
    private Queue<UnitConfig> enemyReserveQueue = new Queue<UnitConfig>();
    private bool isUnitEntering = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (autoStartBattle)
        {
            StartBattle();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }
    
    /// <summary>
    /// 重载当前场景
    /// </summary>
    public void ReloadScene()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 开始生成战斗场景
    /// </summary>
    public void StartBattle()
    {
        DebugLog("[BattleGenerator] ========== 开始生成战斗 ==========");
        
        if (playerConfigs.Count != 3)
        {
            Debug.LogError($"[BattleGenerator] 友方单位必须为3个，当前: {playerConfigs.Count}");
            return;
        }
        
        if (enemyConfigs.Count == 0)
        {
            Debug.LogError("[BattleGenerator] 敌方单位列表为空！");
            return;
        }
        
        StartCoroutine(GenerateBattle());
    }
    
    /// <summary>
    /// 通过代码配置战斗（可选，也可以在Inspector中配置）
    /// </summary>
    public void ConfigureBattle(List<UnitConfig> players, List<UnitConfig> enemies)
    {
        playerConfigs = players;
        enemyConfigs = enemies;
    }
    
    /// <summary>
    /// 快速配置（使用类型名称字符串）
    /// </summary>
    public void ConfigureBattle(string[] playerTypes, string[] enemyTypes)
    {
        playerConfigs.Clear();
        foreach (var type in playerTypes)
            playerConfigs.Add(new UnitConfig(type));
        
        enemyConfigs.Clear();
        foreach (var type in enemyTypes)
            enemyConfigs.Add(new UnitConfig(type));
    }
    
    private IEnumerator GenerateBattle()
    {
        CleanupPreviousBattle();
        
        // 1. 创建敌方单位池
        CreateEnemyPool();
        
        // 2. 生成友方单位
        yield return SpawnPlayerUnits();
        
        // 3. 生成初始敌方单位
        yield return SpawnInitialEnemies();
        
        // 4. 入场动画
        yield return PlayEntranceAnimations();
        
        // 5. 初始化RoundManager
        InitializeRoundManager();
        
        DebugLog("[BattleGenerator] ========== 战斗生成完成 ==========");
    }
    
    private void CreateEnemyPool()
    {
        enemyReserveQueue.Clear();
        foreach (var config in enemyConfigs)
        {
            if (!string.IsNullOrEmpty(config.controllerTypeName))
                enemyReserveQueue.Enqueue(config);
        }
        DebugLog($"[BattleGenerator] 敌方单位池: {enemyReserveQueue.Count}个");
    }
    
    private IEnumerator SpawnPlayerUnits()
    {
        Vector2[] positions = { playerTopPosition, playerMiddlePosition, playerBottomPosition };
        Location[] locations = { Location.Up, Location.Middle, Location.Bottom };
        
        for (int i = 0; i < playerConfigs.Count && i < 3; i++)
        {
            BattleUnit unit = CreateBattleUnitFromConfig(playerConfigs[i], positions[i], Team.Player, locations[i]);
            if (unit != null)
            {
                activePlayerUnits.Add(unit);
                DebugLog($"[BattleGenerator] 生成友方单位: {unit.name}");
            }
            yield return null;
        }
    }
    
    private IEnumerator SpawnInitialEnemies()
    {
        Vector2[] positions = { enemyTopPosition, enemyMiddlePosition, enemyBottomPosition };
        Location[] locations = { Location.Up, Location.Middle, Location.Bottom };
        
        for (int i = 0; i < 3 && enemyReserveQueue.Count > 0; i++)
        {
            UnitConfig config = enemyReserveQueue.Dequeue();
            BattleUnit unit = CreateBattleUnitFromConfig(config, positions[i], Team.Enemy, locations[i]);
            if (unit != null)
            {
                activeEnemyUnits.Add(unit);
                DebugLog($"[BattleGenerator] 生成敌方单位: {unit.name}");
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// 根据配置创建完整的战斗单位GameObject
    /// 自动创建：GameObject → BattleUnit + SpriteRenderer + Controller，并完成接线
    /// </summary>
    private BattleUnit CreateBattleUnitFromConfig(UnitConfig config, Vector2 position, Team team, Location location)
    {
        if (config == null || string.IsNullOrEmpty(config.controllerTypeName))
        {
            Debug.LogError("[BattleGenerator] 配置为空或类型名称为空！");
            return null;
        }
        
        // 1. 查找Controller类型
        Type controllerType = Type.GetType(config.controllerTypeName);
        if (controllerType == null)
        {
            // 尝试在所有程序集中查找
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                controllerType = assembly.GetType(config.controllerTypeName);
                if (controllerType != null) break;
            }
        }
        
        if (controllerType == null)
        {
            Debug.LogError($"[BattleGenerator] 找不到类型: {config.controllerTypeName}");
            return null;
        }
        
        if (!typeof(UnitController).IsAssignableFrom(controllerType))
        {
            Debug.LogError($"[BattleGenerator] {config.controllerTypeName} 不是UnitController的子类！");
            return null;
        }
        
        // 2. 创建GameObject
        GameObject unitObj = new GameObject($"BattleUnit_{config.controllerTypeName}_{location}");
        
        // 3. 设置屏幕外位置
        Vector2 spawnPos = position;
        spawnPos.x += (team == Team.Player ? -offscreenOffset : offscreenOffset);
        unitObj.transform.position = spawnPos;
        
        // 4. 添加Controller组件（Awake会自动从Resources加载Sprite）
        UnitController controller = unitObj.AddComponent(controllerType) as UnitController;
        if (controller == null)
        {
            Debug.LogError($"[BattleGenerator] 无法添加Controller: {config.controllerTypeName}");
            Destroy(unitObj);
            return null;
        }
        
        // 5. 添加SpriteRenderer，从Controller获取Sprite
        SpriteRenderer sr = unitObj.AddComponent<SpriteRenderer>();
        if (controller.CharacterSprite != null)
        {
            sr.sprite = controller.CharacterSprite;
        }
        else
        {
            Debug.LogWarning($"[BattleGenerator] Controller未加载到Sprite: {config.controllerTypeName}");
        }
        
        // 6. 添加BattleUnit组件
        BattleUnit battleUnit = unitObj.AddComponent<BattleUnit>();
        battleUnit.SetTeam(team);
        battleUnit.SetLocation(location);
        
        // 7. 绑定Controller和BattleUnit（自动接线）
        controller.BindUnit(battleUnit);
        
        DebugLog($"[BattleGenerator] 创建完成: {unitObj.name} (Controller: {controllerType.Name})");
        
        return battleUnit;
    }
    
    private IEnumerator PlayEntranceAnimations()
    {
        isUnitEntering = true;
        
        Vector2[] playerTargets = { playerTopPosition, playerMiddlePosition, playerBottomPosition };
        Vector2[] enemyTargets = { enemyTopPosition, enemyMiddlePosition, enemyBottomPosition };
        
        for (int i = 0; i < activePlayerUnits.Count; i++)
            activePlayerUnits[i].transform.DOMove(playerTargets[i], entranceDuration).SetEase(Ease.OutCubic);
        
        for (int i = 0; i < activeEnemyUnits.Count; i++)
            activeEnemyUnits[i].transform.DOMove(enemyTargets[i], entranceDuration).SetEase(Ease.OutCubic);
        
        yield return new WaitForSeconds(entranceDuration);
        isUnitEntering = false;
        
        DebugLog("[BattleGenerator] 入场动画完成");
    }
    
    private void InitializeRoundManager()
    {
        if (RoundManager.Instance == null)
        {
            Debug.LogError("[BattleGenerator] RoundManager不存在！");
            return;
        }
        
        RoundManager.Instance.InitializeBattle(activePlayerUnits, activeEnemyUnits);
        
        foreach (var unit in activeEnemyUnits)
            SubscribeEnemyDeath(unit);
        
        DebugLog("[BattleGenerator] RoundManager初始化完成，战斗开始");
    }
    
    private void SubscribeEnemyDeath(BattleUnit unit)
    {
        unit.OnDeath += () => OnEnemyDeath(unit);
    }
    
    private void OnEnemyDeath(BattleUnit deadUnit)
    {
        activeEnemyUnits.Remove(deadUnit);
        
        if (enemyReserveQueue.Count > 0)
            StartCoroutine(SpawnReplacementEnemy(deadUnit.UnitLocation));
    }
    
    private IEnumerator SpawnReplacementEnemy(Location location)
    {
        if (enemyReserveQueue.Count == 0) yield break;
        
        isUnitEntering = true;
        
        UnitConfig config = enemyReserveQueue.Dequeue();
        Vector2 targetPos = GetPositionByLocation(location);
        
        BattleUnit unit = CreateBattleUnitFromConfig(config, targetPos, Team.Enemy, location);
        if (unit != null)
        {
            activeEnemyUnits.Add(unit);
            
            yield return unit.transform.DOMove(targetPos, entranceDuration).SetEase(Ease.OutCubic).WaitForCompletion();
            
            RoundManager.Instance?.RegisterUnit(unit);
            SubscribeEnemyDeath(unit);
            
            DebugLog($"[BattleGenerator] 替补敌人入场: {unit.name}");
        }
        
        isUnitEntering = false;
    }
    
    private Vector2 GetPositionByLocation(Location location)
    {
        return location switch
        {
            Location.Up => enemyTopPosition,
            Location.Middle => enemyMiddlePosition,
            Location.Bottom => enemyBottomPosition,
            _ => enemyMiddlePosition
        };
    }
    
    private void CleanupPreviousBattle()
    {
        foreach (var unit in activePlayerUnits)
            if (unit != null) Destroy(unit.gameObject);
        activePlayerUnits.Clear();
        
        foreach (var unit in activeEnemyUnits)
            if (unit != null) Destroy(unit.gameObject);
        activeEnemyUnits.Clear();
        
        enemyReserveQueue.Clear();
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs) Debug.Log(message);
    }
    
    public bool IsUnitEntering() => isUnitEntering;
}
