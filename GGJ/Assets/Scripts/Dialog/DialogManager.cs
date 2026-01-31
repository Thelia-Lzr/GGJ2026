using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Text.RegularExpressions;
using System;
using UnityEngine.SceneManagement; 

public class Dialogmanager : MonoBehaviour
{
    [Header("1. 剧情仓库 (按顺序放入所有CSV文件)")]
    public List<TextAsset> allDialogFiles = new List<TextAsset>();

    [Header("2. 立绘配置")]
    public List<string> charNames = new List<string>();
    public List<Sprite> charSprites = new List<Sprite>();
    public Sprite shadowSprite;

    [Header("3. UI 引用")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public UnityEngine.UI.Button nextButton;

    [Header("4. 场景渲染器")]
    public SpriteRenderer imageLeft;
    public SpriteRenderer imageRight;

    [Header("5. 运行状态")]
    public int dialogIndex = 0;
    public float typeSpeed = 0.03f;

    private string[] dialogRows;
    private Coroutine typewriterCoroutine;
    private string currentFullContent; // 用于记录当前完整文字以便跳过打字机

    [Header("6. 入场动画配置")]
    public CanvasGroup dialogCanvasGroup; // 拖入刚才创建的 DialogPanel
    public RectTransform dialogTransform; // 同样拖入 DialogPanel 的 RectTransform

    [Header("7. 背景切换配置")]
    public UnityEngine.UI.Image backgroundDisplay; // 拖入用于显示背景的 UI Image（或者 SpriteRenderer）
    public List<Sprite> backgroundSprites = new List<Sprite>(); // 存放背景图，索引 0, 1, 2...


    void Awake()
    {
        // 游戏开局时，确保 UI 是藏起来的
        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.alpha = 0;
            dialogCanvasGroup.interactable = false;
            dialogCanvasGroup.blocksRaycasts = false; // 隐藏时不遮挡鼠标
        }

        if (imageLeft) imageLeft.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        if (imageRight) imageRight.color = new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnClickNext);

    }

    // --- 核心：外部调用此方法切换剧情 ---
    public void PlayData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= allDialogFiles.Count)
        {
            Debug.LogError($"【剧情错误】尝试播放索引为 {fileIndex} 的剧情，但仓库里没这么多文件！");
            return;
        }

        Debug.Log($"<color=cyan>【系统】开始播放新剧情：{allDialogFiles[fileIndex].name}</color>");
        // 如果 UI 还没显示，先播入场动画
        if (dialogCanvasGroup.alpha < 0.1f)
        {
            ShowPanel();
        }

        // 重置状态
        dialogIndex = 0;
        dialogRows = allDialogFiles[fileIndex].text.Split('\n');

        // 开始播放第一行
        ShowDiaLogRow();
    }

    // 增加一个简单的标记位
    private bool canExit = false;

    public void OnClickNext()
    {
        // 打字机未完时点击：直接显示全文
        // 如果已经播完了，再次点击直接跳转
        if (canExit)
        {
            Debug.Log("【系统】正在跳转场景...");
            // 这里换成你实际的场景名字，或者用索引：SceneManager.GetActiveScene().buildIndex + 1
            return;
        }
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            dialogText.text = currentFullContent;
            return;
        }

        // 打字机已完时点击：下一句
        ShowDiaLogRow();
    }

    public void ShowDiaLogRow()
    {
        if (dialogRows == null) return;
        foreach (string row in dialogRows)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;
            string[] cells = row.Split(',');
            if (cells.Length < 6) continue;

            if (cells[1].Trim() == dialogIndex.ToString())
            {
                if (cells[0].Trim() == "#")
                {
                    // --- 新增：背景切换逻辑 ---
                    if (cells.Length >= 7) // 检查是否有第 7 列
                    {
                        string bgValue = cells[6].Trim();
                        if (!string.IsNullOrEmpty(bgValue))
                        {
                            if (int.TryParse(bgValue, out int bgIndex))
                            {
                                ChangeBackground(bgIndex);
                            }
                        }
                    }
                    UpdateUI(cells[2], cells[4], cells[3]);
                    dialogIndex = int.Parse(cells[5].Trim());
                    return;
                }
            }
            // 检查剧情结束标识
            if (cells[0].Trim() == "end") { EndDialog(); return; }
        }
    }

    private string PureName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return Regex.Replace(raw, @"[^\u4e00-\u9fa5a-zA-Z0-9]", "").Trim();
    }

    private void UpdateUI(string _name, string _content, string _pos)
    {
        string csvPureName = PureName(_name);
        string pos = _pos.Trim();
        currentFullContent = _content.Trim();

        Sprite targetSprite = null;

        for (int i = 0; i < charNames.Count; i++)
        {
            string listPureName = PureName(charNames[i]);
            if (csvPureName != "" && (csvPureName.Contains(listPureName) || listPureName.Contains(csvPureName)))
            {
                if (i < charSprites.Count)
                {
                    targetSprite = charSprites[i];
                    break;
                }
            }
        }

        bool isNarrator = (pos == "中") || csvPureName.Contains("旁白");
        if (targetSprite == null && !isNarrator) targetSprite = shadowSprite;

        nameText.text = _name.Trim();
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        string displayText = isNarrator ? $"【{_name.Trim()}】：{currentFullContent}" : currentFullContent;
        typewriterCoroutine = StartCoroutine(TypeText(displayText));

        ApplyRender(targetSprite, pos, isNarrator);
    }

    private void ApplyRender(Sprite sprite, string pos, bool isNarrator)
    {
        if (imageLeft == null || imageRight == null) return;
        imageLeft.DOKill(); imageRight.DOKill();
        Color active = Color.white;
        Color inactive = new Color(0.3f, 0.3f, 0.3f, 1f);

        if (isNarrator)
        {
            imageLeft.DOColor(inactive, 0.2f); imageRight.DOColor(inactive, 0.2f);
        }
        else if (pos == "左")
        {
            if (sprite != null) imageLeft.sprite = sprite;
            imageLeft.DOColor(active, 0.2f); imageRight.DOColor(inactive, 0.2f);
        }
        else if (pos == "右")
        {
            if (sprite != null) imageRight.sprite = sprite;
            imageRight.DOColor(active, 0.2f); imageLeft.DOColor(inactive, 0.2f);
        }
    }

    IEnumerator TypeText(string t)
    {
        dialogText.text = "";
        foreach (char c in t) { dialogText.text += c; yield return new WaitForSeconds(typeSpeed); }
        typewriterCoroutine = null;
    }

    public void EndDialog()
    {

        dialogText.text = "（剧情结束，点击继续跳转）";
        canExit = true; // 激活跳转门槛

    }
    private void ShowPanel()
    {
        if (dialogCanvasGroup == null || dialogTransform == null) return;

        // 1. 动画前：彻底关闭交互，防止动画期间误点
        dialogCanvasGroup.alpha = 0;
        dialogCanvasGroup.interactable = false;
        dialogCanvasGroup.blocksRaycasts = false; // 这一行很关键，决定了鼠标能不能“穿透”到它身上
        dialogTransform.anchoredPosition = new Vector2(0, -100f);

        // 2. 动画开始
        dialogCanvasGroup.DOFade(1, 0.5f);
        dialogTransform.DOAnchorPos(Vector2.zero, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                // 3. 动画结束：【这里是重点】全部打开
                dialogCanvasGroup.interactable = true;  // 允许点击
                dialogCanvasGroup.blocksRaycasts = true; // 允许射线检测（接收点击）
                Debug.Log("<color=green>【UI系统】动画完成，按钮已激活</color>");
            });
    }
    private void ChangeBackground(int index)
    {
        if (backgroundDisplay == null) return;
        if (index < 0 || index >= backgroundSprites.Count)
        {
            Debug.LogWarning($"【背景系统】尝试切换索引为 {index} 的背景，但列表里没那么多图！");
            return;
        }

        // 如果背景已经是一样的，就不重复切了
        if (backgroundDisplay.sprite == backgroundSprites[index]) return;

        // 增加一个简单的淡入淡出转场效果
        backgroundDisplay.DOKill();
        backgroundDisplay.DOColor(Color.black, 0.3f).OnComplete(() => {
            backgroundDisplay.sprite = backgroundSprites[index];
            backgroundDisplay.DOColor(Color.white, 0.5f);
        });
    }
}