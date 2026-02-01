using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // 这个函数将绑定到按钮上
    public void ExitGame()
    {
        Debug.Log("游戏正在退出...");

        // 如果是在 Unity 编辑器里运行，则停止播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 如果是正式打包后的程序，则执行退出命令
            Application.Quit();
#endif
    }
}