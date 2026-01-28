using UnityEngine;
using UnityEngine.UI;

public class AimingStatusUI : MonoBehaviour
{
    [Header("组件引用")]
    public PlayerAiming aimingSystem;
    public Text statusText;

    [Header("显示设置")]
    public bool showInEditor = true;
    public float updateInterval = 0.1f; // 更新频率

    private float timer;
    private Color lastCrosshairColor;

    void Start()
    {
        if (statusText == null)
            statusText = GetComponent<Text>();

        if (aimingSystem == null)
            aimingSystem = FindObjectOfType<PlayerAiming>();

        if (statusText != null && !showInEditor)
            statusText.enabled = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateStatusDisplay();
            timer = 0f;
        }
    }

    void UpdateStatusDisplay()
    {
        if (aimingSystem == null || statusText == null) return;

        GameObject target = aimingSystem.GetCurrentAimedObject();
        string crosshairStatus = GetCrosshairStatus();

        if (target != null)
        {
            RaycastHit hit;
            if (aimingSystem.GetAimHitInfo(out hit))
            {
                statusText.text =
                    $"<color=green>✓ 瞄准系统正常</color>\n" +
                    $"目标: <b>{target.name}</b>\n" +
                    $"层级: {LayerMask.LayerToName(target.layer)}\n" +
                    $"距离: {hit.distance:F2}米\n" +
                    $"准心: {crosshairStatus}";
            }
        }
        else
        {
            statusText.text =
                $"<color=#AAAAAA>◌ 等待瞄准目标</color>\n" +
                $"准心: {crosshairStatus}\n" +
                $"检测距离: {aimingSystem.maxAimDistance}米\n" +
                $"检测层级: {aimingSystem.aimLayerMask.value}";
        }
    }

    string GetCrosshairStatus()
    {
        if (aimingSystem.crosshairUI != null)
        {
            var image = aimingSystem.crosshairUI.GetComponent<Image>();
            if (image != null)
            {
                if (image.color == aimingSystem.canInteractColor)
                    return "<color=green>绿色 (可交互)</color>";
                else if (image.color == aimingSystem.defaultColor)
                    return "<color=white>白色 (默认)</color>";
                else
                    return $"<color=yellow>其他 ({image.color})</color>";
            }
        }
        return "<color=red>未连接</color>";
    }

    // 编辑器辅助方法
    void OnValidate()
    {
        if (statusText != null)
            statusText.enabled = showInEditor;
    }
}