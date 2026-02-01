using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private int currentStoryStep = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 这里的名字要和 DialogManager 调用的保持一致 ---

    // 获取当前索引
    public int GetCurrentStoryIndex()
    {
        return currentStoryStep;
    }

    // 进度自增（在剧情点完后调用）
    public void AdvanceStoryIndex()
    {
        currentStoryStep++;
    }

    // 兼容你之前的写法，如果 DialogManager 还在报那个错，就加上这个：
    public int GetAndIncrementStoryIndex()
    {
        int index = currentStoryStep;
        currentStoryStep++;
        return index;
    }

    // 重置进度
    public void ResetStoryProgress()
    {
        currentStoryStep = 0;
    }

    public void GoToBattle()
    {
        SceneManager.LoadScene(3); // 确保 3 是你的战斗场景索引
    }

    public void GoToNextStory()
    {
        SceneManager.LoadScene(1); // 确保 1 是你的剧情场景索引
    }
}