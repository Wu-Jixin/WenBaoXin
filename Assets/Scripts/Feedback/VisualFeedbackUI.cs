using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualFeedbackUI : MonoBehaviour
{
    public static VisualFeedbackUI Instance;

    [Header("准心设置")]
    public Image crosshair;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public Color missColor = Color.blue;
    public Color cooldownColor = Color.gray;

    [Header("屏幕震动")]
    public Transform cameraTransform;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;
    private Vector3 originalCameraLocalPosition;
    private Coroutine activeShakeCoroutine;

    [Header("伤害数字")]
    public GameObject damageTextPrefab;  // 绑定一个带 Text 的 prefab
    public Canvas worldCanvas;           // 绑定 Canvas 用于显示数字

    // Coroutine 控制准心反馈
    private Coroutine crosshairCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (cameraTransform != null) originalCameraLocalPosition = cameraTransform.localPosition;
        if (crosshair != null) crosshair.color = normalColor;
    }

    // ================== 准心反馈 ==================
    public void ShowHitFeedback()
    {
        if (crosshair == null) return;
        if (crosshairCoroutine != null) StopCoroutine(crosshairCoroutine);
        crosshairCoroutine = StartCoroutine(CrosshairFeedbackRoutine(hitColor, 1.3f, 0.1f));
    }

    public void ShowMissFeedback()
    {
        if (crosshair == null) return;
        if (crosshairCoroutine != null) StopCoroutine(crosshairCoroutine);
        crosshairCoroutine = StartCoroutine(CrosshairFeedbackRoutine(missColor, 1.1f, 0.2f));
    }

    public void ShowCooldownFeedback(float remainingTime)
    {
        if (crosshair == null) return;
        if (crosshairCoroutine != null) StopCoroutine(crosshairCoroutine);
        crosshairCoroutine = StartCoroutine(CrosshairFeedbackRoutine(cooldownColor, 0.9f, remainingTime));
    }

    private IEnumerator CrosshairFeedbackRoutine(Color targetColor, float targetScale, float duration)
    {
        crosshair.color = targetColor;
        crosshair.transform.localScale = Vector3.one * targetScale;

        yield return new WaitForSeconds(duration);

        crosshair.color = normalColor;
        crosshair.transform.localScale = Vector3.one;

        crosshairCoroutine = null;
    }

    // ================== 屏幕震动 ==================
    public void ShakeCamera(float intensityMultiplier = 1f)
    {
        if (cameraTransform == null) return;
        if (activeShakeCoroutine != null)
        {
            StopCoroutine(activeShakeCoroutine);
            cameraTransform.localPosition = originalCameraLocalPosition;
        }
        activeShakeCoroutine = StartCoroutine(ShakeCoroutine(shakeDuration, shakeIntensity * intensityMultiplier));
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = new Vector3(
                Random.Range(-intensity, intensity),
                0f,
                Random.Range(-intensity, intensity)
            );
            cameraTransform.localPosition = originalCameraLocalPosition + offset;
            yield return null;
        }
        cameraTransform.localPosition = originalCameraLocalPosition;
        activeShakeCoroutine = null;
    }

    // ================== 伤害数字 ==================
    public void ShowDamageNumber(Vector3 worldPosition, float damage)
    {
        if (damageTextPrefab == null || worldCanvas == null)
        {
            Debug.Log($"Damage: {damage}");
            return;
        }

        GameObject damageText = Instantiate(damageTextPrefab, worldCanvas.transform);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 0.5f);
        damageText.transform.position = screenPos;

        Text textComp = damageText.GetComponent<Text>();
        if (textComp != null)
        {
            textComp.text = damage.ToString("F0");

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

        Destroy(damageText, 1f);
    }

    // ================== 工具方法 ==================
    public void UpdateCameraReference(Transform newCamera)
    {
        cameraTransform = newCamera;
        if (cameraTransform != null) originalCameraLocalPosition = cameraTransform.localPosition;
    }
}