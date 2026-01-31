using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class Dialogmanager : MonoBehaviour
{
    [Header("1. 剧情仓库")]
    public List<TextAsset> allDialogFiles = new List<TextAsset>();

    [Header("2. 立绘配置")]
    public List<string> charNames = new List<string>();
    public List<Sprite> charSprites = new List<Sprite>();
    public Sprite shadowSprite;

    [Header("3. UI 引用")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Button nextButton;

    [Header("4. 场景渲染器")]
    public SpriteRenderer imageLeft;
    public SpriteRenderer imageRight;

    [Header("5. 运行状态")]
    public int dialogIndex = 0;
    public float typeSpeed = 0.03f;
    private bool isFinished = false;

    private string[] dialogRows;
    private Coroutine typewriterCoroutine;
    private string currentFullContent;

    [Header("6. 背景切换")]
    public Image backgroundDisplay;
    public List<Sprite> backgroundSprites = new List<Sprite>();

    void Awake()
    {
        if (imageLeft) imageLeft.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        if (imageRight) imageRight.color = new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
    }

    public void PlayData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= allDialogFiles.Count) return;

        isFinished = false;
        dialogIndex = 0;
        dialogRows = allDialogFiles[fileIndex].text.Split('\n');
        ShowDiaLogRow();
    }

    public void OnClickNext()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SfxType.UI, "sfx_ui_click_button");
        }

        if (isFinished)
        {
            Debug.Log("<color=orange>【逻辑测试】跳转至战斗场景</color>");
            return;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            dialogText.text = currentFullContent;
            return;
        }

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
                    if (cells.Length >= 7)
                    {
                        if (int.TryParse(cells[6].Trim(), out int bgIndex)) ChangeBackground(bgIndex);
                    }

                    UpdateUI(cells[2], cells[4], cells[3]);

                    string nextTarget = cells[5].Trim();
                    if (nextTarget.ToLower() == "end")
                    {
                        isFinished = true;
                    }
                    else
                    {
                        int.TryParse(nextTarget, out dialogIndex);
                    }
                    return;
                }
            }
        }
    }

    private void UpdateUI(string _name, string _content, string _pos)
    {
        string csvPureName = PureName(_name);
        string pos = _pos.Trim();
        currentFullContent = _content.Trim();

        Sprite targetSprite = null;
        for (int i = 0; i < charNames.Count; i++)
        {
            if (csvPureName != "" && (csvPureName.Contains(PureName(charNames[i]))))
            {
                if (i < charSprites.Count) { targetSprite = charSprites[i]; break; }
            }
        }

        bool isNarrator = (pos == "中") || csvPureName.Contains("旁白");

        // 调用兜底：如果没找到对应立绘且不是旁白，就用 shadowSprite
        if (targetSprite == null && !isNarrator) targetSprite = shadowSprite;

        nameText.text = _name.Trim();
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        string displayText = isNarrator ? $"【{_name.Trim()}】：{currentFullContent}" : currentFullContent;
        typewriterCoroutine = StartCoroutine(TypeText(displayText));

        ApplyRender(targetSprite, pos, isNarrator);

        if (dialogIndex == 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalBGM("Plot");
        }
    }

    private void ApplyRender(Sprite sprite, string pos, bool isNarrator)
    {
        if (imageLeft == null || imageRight == null) return;
        imageLeft.DOKill(); imageRight.DOKill();
        Color active = Color.white;
        Color inactive = new Color(0.3f, 0.3f, 0.3f, 1f);

        // 1. 定义两个精准的缩放值
        Vector3 playerScale = new Vector3(0.6f, 0.6f, 1f); // 你原本主角的缩放
        Vector3 shadowScale = new Vector3(1f, 1.15f, 1f); // 杂鱼放大的缩放 (根据需要调这个数)

        // 2. 判定：当前传入的是否是兜底杂鱼
        bool isShadow = (sprite != null && shadowSprite != null && sprite.name == shadowSprite.name);

        if (isNarrator)
        {
            imageLeft.DOColor(inactive, 0.2f); imageRight.DOColor(inactive, 0.2f);
        }
        else if (pos == "左")
        {
            if (sprite != null) imageLeft.sprite = sprite;
            imageLeft.DOColor(active, 0.2f);
            imageRight.DOColor(inactive, 0.2f);

            // 核心逻辑：是杂鱼就用 1.2，不是杂鱼（主角）就强制回 0.6
            imageLeft.transform.localScale = isShadow ? shadowScale : playerScale;
        }
        else if (pos == "右")
        {
            if (sprite != null) imageRight.sprite = sprite;
            imageRight.DOColor(active, 0.2f);
            imageLeft.DOColor(inactive, 0.2f);

            // 核心逻辑：同上
            imageRight.transform.localScale = isShadow ? shadowScale : playerScale;
        }
    }

    private void ChangeBackground(int index)
    {
        if (backgroundDisplay == null || index < 0 || index >= backgroundSprites.Count) return;
        if (backgroundDisplay.sprite == backgroundSprites[index]) return;

        backgroundDisplay.DOKill();
        backgroundDisplay.DOColor(Color.black, 0.3f).OnComplete(() => {
            backgroundDisplay.sprite = backgroundSprites[index];
            backgroundDisplay.DOColor(Color.white, 0.5f);
        });
    }

    IEnumerator TypeText(string t)
    {
        dialogText.text = "";
        foreach (char c in t) { dialogText.text += c; yield return new WaitForSeconds(typeSpeed); }
        typewriterCoroutine = null;
    }

    public void EndDialog()
    {
        isFinished = true;
        dialogText.text = "（剧情结束，点击继续）";
    }

    private string PureName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return Regex.Replace(raw, @"[^\u4e00-\u9fa5a-zA-Z0-9]", "").Trim();
    }
}