using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("===== 工具UI =====")]
    public TMP_Text toolText;     // 左上角显示当前工具
    public Image toolIcon;        // 工具图标（可选）

    [Header("===== 提示面板 =====")]
    public GameObject guidancePanel;

    [Header("提示文本")]
    public TMP_Text guidanceText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }

        // 初始工具显示
        UpdateToolUI(null, "无");
    }

    void Update()
    {
        // 测试提示系统
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowGuidance("工具使用错误！");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ShowGuidance("操作正确！");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            HideGuidance();
        }
    }

    //==============================
    // 工具UI更新
    //==============================

    public void UpdateToolUI(Sprite icon, string toolName)
    {
        if (toolText != null)
        {
            toolText.text = "当前工具：" + toolName;
        }

        if (toolIcon != null && icon != null)
        {
            toolIcon.sprite = icon;
        }

        Debug.Log("当前工具UI更新: " + toolName);
    }

    //==============================
    // 提示系统
    //==============================

    public void ShowGuidance(string message)
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(true);
        }

        if (guidanceText != null)
        {
            guidanceText.text = message;
        }

        Debug.Log("提示显示: " + message);
    }

    public void HideGuidance()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }

        Debug.Log("提示关闭");
    }
}