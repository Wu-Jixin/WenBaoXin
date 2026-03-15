using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuidanceManager : MonoBehaviour
{
    // =============================
    // 单例 Instance
    // =============================
    public static GuidanceManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =============================
    // UI引用
    // =============================

    [Header("Guidance UI")]
    public GameObject guidancePanel;

    public TextMeshProUGUI guidanceText;

    // =============================
    // 设置
    // =============================

    [Header("Settings")]
    public float autoHideTime = 5f;

    private float hideTimer;

    private bool isShowing = false;

    // =============================
    // 初始化
    // =============================

    void Start()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }
    }

    // =============================
    // Update
    // =============================

    void Update()
    {
        if (isShowing)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0)
            {
                HideGuidance();
            }
        }
    }

    // =============================
    // 显示提示
    // =============================

    public void ShowGuidance(string message)
    {
        if (guidancePanel == null || guidanceText == null)
        {
            Debug.LogWarning("Guidance UI 未绑定");
            return;
        }

        guidancePanel.SetActive(true);

        guidanceText.text = message;

        hideTimer = autoHideTime;

        isShowing = true;

        Debug.Log("Guidance: " + message);
    }

    // =============================
    // 显示指定时间
    // =============================

    public void ShowGuidance(string message, float duration)
    {
        if (guidancePanel == null || guidanceText == null)
        {
            Debug.LogWarning("Guidance UI 未绑定");
            return;
        }

        guidancePanel.SetActive(true);

        guidanceText.text = message;

        hideTimer = duration;

        isShowing = true;

        Debug.Log("Guidance: " + message);
    }

    // =============================
    // 隐藏提示
    // =============================

    public void HideGuidance()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }

        isShowing = false;
    }

    // =============================
    // 强制关闭
    // =============================

    public void ForceHide()
    {
        HideGuidance();

        hideTimer = 0;
    }
}