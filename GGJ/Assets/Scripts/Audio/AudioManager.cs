using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ��ȫ����Ƶ��������
/// ְ��ͳһ���� BGM������ʽ/��ͨѭ������ SFX ����ء�
/// Э��˵����
/// 1. BGM���� Inspector ��Ӧ������ BGM �б������ó�����Ƶ��
/// 2. SFX���� SfxCategory �б���ͨ�����ֹ�����Ч��֧�֡�һ�����Ƶ��������š�
/// 3. ���ã�BGM ʹ�ó��������ã�SFX ʹ�÷���ö�ٻ����Ƭ�������á�
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    #region --- ���ݽṹ���� ---

    /// <summary> ���� 1������ʽ BGM��Intro + Loop�� </summary>
    [System.Serializable]
    public class TwoPartBgmData
    {
        public string bgmName;      // ����ʶ�������� "Lobby"
        public AudioClip introClip; // ��ͷ��
        public AudioClip loopClip;  // ѭ����
    }

    /// <summary> ���� 2����ͨѭ�� BGM��ֱ�� Loop�� </summary>
    [System.Serializable]
    public class NormalBgmData
    {
        public string bgmName;      // ����ʶ�������� "Plot"
        public AudioClip clip;      // ѭ����Ƶ
    }

    /// <summary> SFX ���������ͨ����������Ƶ��֧�ְ������� </summary>
    [System.Serializable]
    public class SfxCategory
    {
        public SfxType sfxType;           // ����ö��
        public List<AudioClip> clips;     // �÷����µ�������Ƶ�ļ�
    }

    public enum SfxType
    {
        Attack,     // �������ӡ��������й�����
        Hurt,       // �ܻ�
        Card,       // ���ơ�ѡ��
        Mask,       // ���װ��������
        Skill,      // ���桢��Ч
        UI          // ���������
    }
    #endregion

    #region --- Inspector ��Դ�б� ---

    [Header("BGM ���� 1������ʽ (Intro + Loop)")]
    public List<TwoPartBgmData> twoPartBgmList;

    [Header("BGM ���� 2����ͨѭ�� (Normal Loop)")]
    public List<NormalBgmData> normalBgmList;

    [Header("SFX ������Դ�� (֧�ְ�������)")]
    public List<SfxCategory> sfxCategories;

    [Header("ȫ����������")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("��������")]
    [SerializeField] private AudioSource bgmSource;   // ���볡���еı�������Դ
    [SerializeField] private AudioSource sfxPrefab;   // ������ЧԤ���� (��AudioSource���)
    #endregion

    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private string _currentBgmName = "";

    #region --- �������ýӿ� ---

    /// <summary> ��������ʽ BGM </summary>
    public void PlayTwoPartBGM(string bgmName)
    {
        TwoPartBgmData data = twoPartBgmList.Find(b => b.bgmName == bgmName);
        if (data == null) { Debug.LogError($"[Audio] δ�ҵ�����ʽBGM: {bgmName}"); return; }
        ExecuteBgmPlay(data.introClip, data.loopClip, bgmName, true);
    }

    /// <summary> ������ͨѭ�� BGM </summary>
    public void PlayNormalBGM(string bgmName)
    {
        NormalBgmData data = normalBgmList.Find(b => b.bgmName == bgmName);
        if (data == null) { Debug.LogError($"[Audio] δ�ҵ���ͨBGM: {bgmName}"); return; }
        ExecuteBgmPlay(null, data.clip, bgmName, false);
    }

    /// <summary> 
    /// ���ݷ������Ƶ�ļ���������Ч
    /// ����ʾ����PlaySFX(SfxType.Attack, "sfx_atk_hammer");
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
        Debug.LogWarning($"[Audio] �ڷ��� {type} ���Ҳ�����Ϊ {clipName} ����Ч");
    }

    /// <summary> ������ŷ����µ�һ����Ч�������ڶ����ܻ�������л��� </summary>
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

    #region --- ���ĵײ��߼� ---

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitializePool(); }
        else { Destroy(gameObject); }
    }

    private void InitializePool()
    {
        if (sfxPrefab == null) { Debug.LogError("����Inspector��ָ��Sfx Prefab"); return; }
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
           source.pitch = Random.Range(0.95f, 1.05f); // ���΢�����������Ӵ����
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