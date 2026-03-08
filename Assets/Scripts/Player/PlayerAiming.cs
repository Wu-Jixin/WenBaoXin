using UnityEngine;
using UnityEngine.UI;

public class PlayerAiming : MonoBehaviour
{
    [Header("准心绑定")]
    public GameObject crosshairUI; // Inspector 拖 Crosshair GameObject

    void Start()
    {
        // 自动同步准心到 VisualFeedbackUI
        if (crosshairUI != null)
        {
            Image img = crosshairUI.GetComponent<Image>();
            if (VisualFeedbackUI.Instance != null && img != null)
            {
                VisualFeedbackUI.Instance.crosshair = img;
                Debug.Log("✅ 已同步准心到 VisualFeedbackUI");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerAiming: crosshairUI 未绑定！");
        }
    }

    void Update()
    {
        // =======================
        // 测试准心反馈（安全键位，不冲突）
        // Z -> Hit, X -> Miss, C -> Cooldown
        // =======================
        if (Input.GetKeyDown(KeyCode.Z))
        {
            VisualFeedbackUI.Instance?.ShowHitFeedback();
            Debug.Log("准心命中效果触发 (Z)");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            VisualFeedbackUI.Instance?.ShowMissFeedback();
            Debug.Log("准心未命中效果触发 (X)");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            VisualFeedbackUI.Instance?.ShowCooldownFeedback(0.5f);
            Debug.Log("准心冷却效果触发 (C)");
        }
    }
}