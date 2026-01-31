using UnityEngine;
using UnityEngine.EventSystems;

public class TestDialogTrigger : MonoBehaviour
{
    public Dialogmanager dialogManager;

    void Update()
    {
        // 1. 监控任何按键按下
        if (Input.anyKeyDown)
        {
            Debug.Log($"<color=white>【底层捕获】 检测到按键: {Input.inputString}</color>");
        }

        // 2. 检查 EventSystem 是否正在选中某个输入框导致按键失效
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            // 如果控制台一直刷这行，说明你的 UI 抢走了键盘焦点
            // Debug.Log("当前焦点被 UI 占用: " + EventSystem.current.currentSelectedGameObject.name);
        }

        // 3. 强制检测大键盘和小键盘的 1
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            TriggerPlay(0);
        }
    }

    void TriggerPlay(int index)
    {
        Debug.Log($"<color=green>【指令发出】 准备播放剧情索引: {index}</color>");
        if (dialogManager != null) dialogManager.PlayData(index);
        else Debug.LogError("DialogManager 引用缺失！");
    }

    // 4. 备用：在屏幕上画按钮，防止键盘死活不灵
    private void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 120, 40), "测试剧情 0")) TriggerPlay(0);
        if (GUI.Button(new Rect(20, 70, 120, 40), "测试剧情 1")) TriggerPlay(1);
    }
}