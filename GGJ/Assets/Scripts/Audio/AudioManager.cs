using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 【全局音频管理器】
/// 职责：统一管理 BGM（两段式/普通循环）与 SFX 对象池。
/// 协作说明：
/// 1. BGM：在 Inspector 对应的两个 BGM 列表中配置场景音频。
/// 2. SFX：在 SfxCategory 列表中通过名字管理音效，支持“一类多音频”随机播放。
/// 3. 调用：BGM 使用场景名调用，SFX 使用分类枚举或具体片段名调用。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    #region --- 数据结构定义 ---

    /// <summary> 类型 1：两段式 BGM（Intro + Loop） </summary>
    [System.Serializable]
    public class TwoPartBgmData
    {
        public string bgmName;      // 场景识别名，如 "Lobby"
        public AudioClip introClip; // 开头段
        public AudioClip loopClip;  // 循环段
    }

    /// <summary> 类型 2：普通循环 BGM（直接 Loop） </summary>
    [System.Serializable]
    public class NormalBgmData
    {
        public string bgmName;      // 场景识别名，如 "Plot"
        public AudioClip clip;      // 循环音频
    }

    /// <summary> SFX 分类管理：通过分类存放音频，支持按名查找 </summary>
    [System.Serializable]
    public class SfxCategory
    {
        public SfxType sfxType;           // 分类枚举
        public List<AudioClip> clips;     // 该分类下的所有音频文件
    }

    public enum SfxType
    {
        Attack,     // 包含锤子、刀等所有攻击声
        Hurt,       // 受击
        Card,       // 抽牌、选牌
        Mask,       // 面具装备、破碎
        Skill,      // 增益、特效
        UI          // 点击、交互
    }
    #endregion

    #region --- Inspector 资源列表 ---

    [Header("BGM 类型 1：两段式 (Intro + Loop)")]
    public List<TwoPartBgmData> twoPartBgmList;

    [Header("BGM 类型 2：普通循环 (Normal Loop)")]
    public List<NormalBgmData> normalBgmList;

    [Header("SFX 分类资源库 (支持按名查找)")]
    public List<SfxCategory> sfxCategories;

    [Header("全局音量配置")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("核心引用")]
    [SerializeField] private AudioSource bgmSource;   // 拖入场景中的背景音乐源
    [SerializeField] private AudioSource sfxPrefab;   // 拖入音效预制体 (带AudioSource组件)
    #endregion

    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private string _currentBgmName = "";

    #region --- 公开调用接口 ---

    /// <summary> 播放两段式 BGM </summary>
    public void PlayTwoPartBGM(string bgmName)
    {
        TwoPartBgmData data = twoPartBgmList.Find(b => b.bgmName == bgmName);
        if (data == null) { Debug.LogError($"[Audio] 未找到两段式BGM: {bgmName}"); return; }
        ExecuteBgmPlay(data.introClip, data.loopClip, bgmName, true);
    }

    /// <summary> 播放普通循环 BGM </summary>
    public void PlayNormalBGM(string bgmName)
    {
        NormalBgmData data = normalBgmList.Find(b => b.bgmName == bgmName);
        if (data == null) { Debug.LogError($"[Audio] 未找到普通BGM: {bgmName}"); return; }
        ExecuteBgmPlay(null, data.clip, bgmName, false);
    }

    /// <summary> 
    /// 根据分类和音频文件名播放音效
    /// 调用示例：PlaySFX(SfxType.Attack, "sfx_atk_hammer");
    /// </summary>
    public void PlaySFX(SfxType type, string clipName)
    {
        SfxCategory category = sfxCategories.Find(s => s.sfxType == type);
        if (category != null)
        {
            AudioClip targetClip = category.clips.Find(c => c.name == clipName);
            if (targetClip != null)
            {
                StartCoroutine(PlaySfxRoutine(targetClip));
                return;
            }
        }
        Debug.LogWarning($"[Audio] 在分类 {type} 中找不到名为 {clipName} 的音效");
    }

    /// <summary> 随机播放分类下的一个音效（适用于多种受击声随机切换） </summary>
    public void PlayRandomSFX(SfxType type)
    {
        SfxCategory category = sfxCategories.Find(s => s.sfxType == type);
        if (category != null && category.clips.Count > 0)
        {
            int randomIndex = Random.Range(0, category.clips.Count);
            StartCoroutine(PlaySfxRoutine(category.clips[randomIndex]));
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        _currentBgmName = "";
        StopAllCoroutines();
    }
    #endregion

    #region --- 核心底层逻辑 ---

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitializePool(); }
        else { Destroy(gameObject); }


    }

    private void InitializePool()
    {
        if (sfxPrefab == null) { Debug.LogError("请在Inspector中指定Sfx Prefab"); return; }
        for (int i = 0; i < 10; i++) CreatePoolObject();
    }

    private AudioSource CreatePoolObject()
    {
        AudioSource src = Instantiate(sfxPrefab, transform);
        src.gameObject.SetActive(false);
        _sfxPool.Add(src);
        return src;
    }

    private void ExecuteBgmPlay(AudioClip intro, AudioClip loop, string bgmName, bool isTwoPart)
    {
        if (_currentBgmName == bgmName) return;

        StopAllCoroutines();
        bgmSource.Stop();
        _currentBgmName = bgmName;

        if (isTwoPart && intro != null)
        {
            bgmSource.clip = intro;
            bgmSource.loop = false;
            bgmSource.Play();
            StartCoroutine(QueueLoopPart(loop));
        }
        else
        {
            bgmSource.clip = loop;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private IEnumerator QueueLoopPart(AudioClip loopClip)
    {
        yield return new WaitForSeconds(bgmSource.clip.length);
        bgmSource.clip = loopClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private IEnumerator PlaySfxRoutine(AudioClip clip)
       {
           AudioSource source = GetAvailableSFX();
           source.clip = clip;
           source.volume = sfxVolume * masterVolume;
           source.pitch = Random.Range(0.95f, 1.05f); // 随机微调音调，增加打击感
           source.Play();
           yield return new WaitForSeconds(clip.length);
           if (source != null) source.gameObject.SetActive(false);
       }


    private AudioSource GetAvailableSFX()
    {
        foreach (var src in _sfxPool)
        {
            if (src != null && !src.gameObject.activeInHierarchy)
            {
                src.gameObject.SetActive(true);
                return src;
            }
        }
        return CreatePoolObject();
    }
    #endregion
}