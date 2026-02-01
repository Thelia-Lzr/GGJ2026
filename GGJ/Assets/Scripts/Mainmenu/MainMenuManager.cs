using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // 1. 进入主菜单即播放 BGM
        if (AudioManager.Instance != null)
        {
            // 确保你在 AudioManager 的列表中配置了 "MainMenu" 这个名字
            AudioManager.Instance.PlayTwoPartBGM("Lobby");
        }
    }


    public void OnClickStart()
    {
        Debug.Log(">>> 1. 按钮确实被点到了！");

        if (SceneController.Instance != null)
        {
            Debug.Log(">>> 2. 找到了 SceneController 实例！");
            SceneController.Instance.ResetStoryProgress();

            Debug.Log(">>> 3. 准备跳转到场景 2...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError(">>> 2. 报错：找不到 SceneController 实例！请务必从 Init 场景启动！");
        }
    }

    // 绑定到“退出游戏”按钮
    public void OnClickQuit()
    {
        PlayClickSound();
        Debug.Log("退出游戏");
        Application.Quit(); // 仅在打包后有效
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SfxType.UI, "sfx_ui_click");
        }
    }
}