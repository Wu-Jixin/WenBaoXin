using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualFeedbackUI : MonoBehaviour
{
    public static VisualFeedbackUI Instance;

    [Header("准心反馈")]
    public Image crosshair;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public Color missColor = Color.blue;
    public Color cooldownColor = Color.gray;

    [Header("屏幕震动")]
    public Transform cameraTransform;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    [Header("伤害数字")]
    public GameObject damageTextPrefab;
    public Canvas worldCanvas;

    // 保存摄像机的原始本地位置
    private Vector3 originalCameraLocalPosition;
    private PlayerController playerController;
    private Coroutine activeShakeCoroutine;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("VisualFeedbackUI 实例创建成功");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 获取摄像机引用
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            Debug.Log($"自动获取摄像机: {cameraTransform}");
        }
    }

    void Start()
    {
        // 保存摄像机原始位置
        if (cameraTransform != null)
        {
            originalCameraLocalPosition = cameraTransform.localPosition;
        }

        // 获取PlayerController引用
        playerController = FindObjectOfType<PlayerController>();
        Debug.Log($"找到 PlayerController: {playerController != null}");

        // 初始化准心颜色
        if (crosshair != null)
        {
            crosshair.color = normalColor;
            Debug.Log($"准心初始化颜色: {crosshair.color}");
        }
        else
        {
            Debug.LogError("crosshair 为 null，请在Inspector中赋值");
        }
    }

    // ========== 准心反馈 ==========

    /// <summary>
    /// 显示命中反馈（红色）
    /// </summary>
    public void ShowHitFeedback()
    {
        if (crosshair == null)
        {
            Debug.LogError("ShowHitFeedback: crosshair 为 null");
            return;
        }

        Debug.Log($"🎯 命中反馈：设置颜色为 {hitColor}");
        crosshair.color = hitColor;
        crosshair.transform.localScale = Vector3.one * 1.3f;

        CancelInvoke("ResetCrosshair");
        Invoke("ResetCrosshair", 0.1f);
    }

    /// <summary>
    /// 显示未命中反馈（蓝色）
    /// </summary>
    public void ShowMissFeedback()
    {
        if (crosshair == null)
        {
            Debug.LogError("ShowMissFeedback: crosshair 为 null");
            return;
        }

        Debug.Log($"🎯 未命中反馈：设置颜色为 {missColor}");
        crosshair.color = missColor;
        crosshair.transform.localScale = Vector3.one * 1.1f;

        CancelInvoke("ResetCrosshair");
        Invoke("ResetCrosshair", 0.2f);
    }

    /// <summary>
    /// 显示冷却反馈（灰色）
    /// </summary>
    public void ShowCooldownFeedback(float remainingTime)
    {
        if (crosshair == null)
        {
            Debug.LogError("ShowCooldownFeedback: crosshair 为 null");
            return;
        }

        Debug.Log($"🎯 冷却反馈：准心变灰，剩余 {remainingTime:F1}秒");
        crosshair.color = cooldownColor;
        crosshair.transform.localScale = Vector3.one * 0.9f;

        CancelInvoke("ResetCrosshair");
        Invoke("ResetCrosshair", 1f);
    }

    /// <summary>
    /// 重置准心
    /// </summary>
    void ResetCrosshair()
    {
        if (crosshair == null) return;

        Debug.Log($"🎯 重置准心为白色");
        crosshair.color = normalColor;
        crosshair.transform.localScale = Vector3.one;
    }

    // ========== 屏幕震动 ==========

    /// <summary>
    /// 屏幕震动 - 只震动XZ轴，不影响Y轴（蹲下高度）
    /// </summary>
    public void ShakeCamera(float intensityMultiplier = 1f)
    {
        if (cameraTransform == null) return;

        // 停止当前震动
        if (activeShakeCoroutine != null)
        {
            StopCoroutine(activeShakeCoroutine);

            // 恢复到震动前的位置
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(
                    cameraTransform.localPosition.x,
                    GetCurrentCameraHeight(),
                    cameraTransform.localPosition.z
                );
            }
        }

        activeShakeCoroutine = StartCoroutine(ShakeCoroutine(
            shakeDuration,
            shakeIntensity * intensityMultiplier
        ));
    }

    /// <summary>
    /// 震动协程
    /// </summary>
    IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        if (cameraTransform == null) yield break;

        // 保存当前Y轴高度（蹲下高度）
        float currentY = GetCurrentCameraHeight();
        Vector3 startLocalPos = cameraTransform.localPosition;
        startLocalPos.y = currentY; // 确保使用正确的高度

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 只在X和Z轴震动，Y轴保持当前高度
            Vector3 shakeOffset = new Vector3(
                Random.Range(-intensity, intensity),
                0f, // Y轴固定为0，不改变摄像机高度
                Random.Range(-intensity, intensity)
            );

            cameraTransform.localPosition = startLocalPos + shakeOffset;

            yield return null;
        }

        // 恢复到震动前的位置（保持当前蹲下高度）
        cameraTransform.localPosition = startLocalPos;
        activeShakeCoroutine = null;
    }

    /// <summary>
    /// 获取当前摄像机应该保持的高度（考虑蹲下状态）
    /// </summary>
    float GetCurrentCameraHeight()
    {
        if (playerController != null)
        {
            // 从PlayerController获取当前摄像机高度
            return cameraTransform.localPosition.y;
        }

        return cameraTransform.localPosition.y;
    }

    // ========== 伤害数字 ==========

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDamageNumber(Vector3 worldPosition, float damage)
    {
        if (damageTextPrefab != null && worldCanvas != null)
        {
            GameObject damageText = Instantiate(damageTextPrefab, worldCanvas.transform);

            // 世界坐标转屏幕坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 0.5f);
            damageText.transform.position = screenPos;

            // 设置文本
            Text textComp = damageText.GetComponent<Text>();
            if (textComp != null)
            {
                textComp.text = damage.ToString("F0");

                // 根据伤害值改变颜色和大小
                if (damage > 50)
                {
                    textComp.color = Color.red;
                    textComp.fontSize = 28;
                }
                else if (damage > 20)
                {
                    textComp.color = Color.yellow;
                    textComp.fontSize = 24;
                }
                else
                {
                    textComp.color = Color.white;
                    textComp.fontSize = 20;
                }
            }

            // 1秒后销毁
            Destroy(damageText, 1f);
        }
        else
        {
            // 如果没有预制体，只在控制台显示
            Debug.Log($"伤害: {damage}");
        }
    }

    // ========== 工具方法 ==========

    /// <summary>
    /// 更新摄像机引用
    /// </summary>
    public void UpdateCameraReference(Transform newCamera)
    {
        cameraTransform = newCamera;

        if (cameraTransform != null)
        {
            originalCameraLocalPosition = cameraTransform.localPosition;
        }
    }

    /// <summary>
    /// 重置摄像机位置
    /// </summary>
    public void ResetCameraPosition()
    {
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraLocalPosition;
        }
    }

    // ========== 调试 ==========

    void OnGUI()
    {
        // 按F12显示调试信息
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Debug.Log("=== VisualFeedbackUI 调试信息 ===");
            Debug.Log($"摄像机: {cameraTransform}");
            Debug.Log($"摄像机位置: {cameraTransform?.localPosition}");
            Debug.Log($"准心颜色: {crosshair?.color}");
            Debug.Log($"震动协程: {activeShakeCoroutine != null}");
        }
    }

    void OnDestroy()
    {
        // 确保摄像机位置恢复
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraLocalPosition;
        }
    }
}