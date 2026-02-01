using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    void Update()
    {
        // 检测按下回车键 (Return/Enter)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("<color=cyan>【战斗模拟】战斗结束，正在申请返回剧情...</color>");

            if (SceneController.Instance != null)
            {
                // 调用你之前写好的跳转方法
                SceneController.Instance.GoToNextStory();
            }
            else
            {
                Debug.LogError("找不到 SceneController！请确保你从主场景启动，或者场景里有这个单例。");
            }
        }
    }

    // 为了方便你在屏幕上看到提示
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 500, 50), "模拟战斗场景：按下【回车】结束战斗并返回剧情", style);
    }
}