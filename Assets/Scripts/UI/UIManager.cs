using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    // ⭐ 当前提示协程（防止闪烁）
    private Coroutine currentGuidanceCoroutine;

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
        // ===== 测试用（可删）=====
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowGuidance("工具使用错误！");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ShowGuidance("操作正确！");
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
    // ✅ 提示系统（稳定版，不闪）
    //==============================

    public void ShowGuidance(string message, float duration = 5f)
    {
        if (currentGuidanceCoroutine != null)
        {
            StopCoroutine(currentGuidanceCoroutine);
        }

        currentGuidanceCoroutine = StartCoroutine(GuidanceCoroutine(message, duration));
    }

    IEnumerator GuidanceCoroutine(string message, float duration)
    {
        if (guidancePanel != null)
            guidancePanel.SetActive(true);

        if (guidanceText != null)
            guidanceText.text = message;

        yield return new WaitForSeconds(duration);

        if (guidancePanel != null)
            guidancePanel.SetActive(false);

        currentGuidanceCoroutine = null;
    }

    //==============================
    // ❗（可选）强制关闭提示
    //==============================
    public void ForceHideGuidance()
    {
        if (currentGuidanceCoroutine != null)
        {
            StopCoroutine(currentGuidanceCoroutine);
            currentGuidanceCoroutine = null;
        }

        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }

        Debug.Log("提示被强制关闭");
    }
}