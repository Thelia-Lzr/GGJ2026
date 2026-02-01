using UnityEngine;

/// <summary>
/// 音频系统全功能诊断测试器
/// </summary>
public class AudioTester : MonoBehaviour
{
    [Header("BGM 测试名称 (需与 Inspector 中一致)")]
    public string lobbyName = "Lobby";
    public string plotName = "Plot";

    [Header("SFX 测试配置")]
    public AudioManager.SfxType attackType = AudioManager.SfxType.Attack;
    public string hammerClipName = "sfx_atk_hammer";

    void Update()
    {
        // --- BGM 测试 ---
        // 按 1：测试两段式 BGM (如：大厅)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log($"<color=cyan>【测试】尝试播放两段式 BGM: {lobbyName}</color>");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayTwoPartBGM(lobbyName);
            else
                Debug.LogError("【错误】场景中未找到 AudioManager 实例！");
        }

        // 按 2：测试普通循环 BGM (如：剧情)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log($"<color=cyan>【测试】尝试播放普通 BGM: {plotName}</color>");
            AudioManager.Instance.PlayNormalBGM(plotName);
        }

        // --- SFX 测试 ---
        // 按 3：测试指定名字的音效（改为数字3，避免与抽卡的空格键冲突）
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log($"<color=yellow>【测试】尝试播放音效: {hammerClipName} (分类: {attackType})</color>");
            AudioManager.Instance.PlaySFX(attackType, hammerClipName);
        }

        // 按 R：测试随机播放分类音效
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"<color=yellow>【测试】随机播放分类 {attackType} 下的音效</color>");
            AudioManager.Instance.PlayRandomSFX(attackType);
        }

        // --- 停止测试 ---
        // 按 S：停止背景音乐
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("<color=white>【测试】停止所有背景音乐</color>");
            AudioManager.Instance.StopBGM();
        }
    }

    // 启动时自动检查底层配置
    void Start()
    {
        Debug.Log("<color=orange>=== 音频系统自检开始 ===</color>");
        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ 自检失败：场景中没有 AudioManager 物体，或脚本未挂载！");
            return;
        }

        // 这里的自检需要保证 AudioManager 里的变量是 public 的才能访问
        // 如果报错，可以暂时注释掉这段 Start 里的内容
        Debug.Log("✅ 自检成功：AudioManager 实例已就绪。");
        Debug.Log("操作指南：[1]两段BGM  [2]普通BGM  [3]指定音效  [R]随机音效  [S]停止");
    }
}