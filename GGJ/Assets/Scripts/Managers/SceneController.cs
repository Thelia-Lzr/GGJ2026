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
            DontDestroyOnLoad(gameObject); // 确保它跨场景存在
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 重点：如果已经有一个了，就把新出来的删掉
        }
    }


    public int GetCurrentStoryIndex()
    {
        return currentStoryStep;
    }
    public void AdvanceStoryIndex()
    {
        currentStoryStep++;
    }


    public int GetAndIncrementStoryIndex()
    {
        int indexToReturn = currentStoryStep;
        currentStoryStep++; // 这里加 1，确保下次再进 Start 拿的是新的
        Debug.Log($"<color=orange>控制器：发放剧本 {indexToReturn}，进度指针已移至 {currentStoryStep}</color>");
        return indexToReturn;
    }

    // 重置进度
    public void ResetStoryProgress()
    {
        currentStoryStep = 0;
    }

    // SceneController.cs
    public void GoToBattle1() { UnityEngine.SceneManagement.SceneManager.LoadScene(2); }
    public void GoToBattle2() { UnityEngine.SceneManagement.SceneManager.LoadScene(3); }
    public void GoToBattle3() { UnityEngine.SceneManagement.SceneManager.LoadScene(4); }

    public void GoToNextStory()
    {
        SceneManager.LoadScene(1); 
    }
    // 跳转到主菜单并重置进度
    // 在 SceneController.cs 中
    public void BackToMainMenu()
    {
        // 1. 进度归零（确保下次进来是从第一章开始）
        currentStoryStep = 0;

        // 2. 强制加载索引 0
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        Debug.Log("<color=red>【系统】退出按钮触发：进度已重置，正在返回主菜单（Index 0）</color>");
    }

    // 供主菜单的“开始游戏”按钮调用
    public void StartNewGame()
    {
        Debug.Log("<color=green>【主菜单】开始新游戏，正在初始化进度...</color>");

        // 1. 确保进度归零
        ResetStoryProgress();

        // 2. 跳转到剧情场景 (索引 1)
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}