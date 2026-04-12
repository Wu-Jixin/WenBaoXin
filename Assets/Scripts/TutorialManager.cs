using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;

    private bool isShowing = true;

    void Start()
    {
        // 游戏开始显示教程
        tutorialPanel.SetActive(true);
        isShowing = true;

        // （可选）暂停游戏
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (isShowing && Input.GetMouseButtonDown(0))
        {
            CloseTutorial();
        }
    }

    void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        isShowing = false;

        // 恢复游戏
        Time.timeScale = 1f;
    }
}