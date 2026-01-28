using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // 添加这个命名空间

public class PlayerAiming : MonoBehaviour
{
    [Header("瞄准设置")]
    public float maxAimDistance = 5f;
    public LayerMask aimLayerMask;

    [Header("准心反馈")]
    public GameObject crosshairUI;
    public Color defaultColor = Color.white;
    public Color canInteractColor = Color.green;

    [Header("调试选项")]
    public bool showDebugRay = true;
    public Color debugRayColor = Color.red;

    // 事件
    public UnityEvent<GameObject> OnAimTargetChanged;

    // 添加一个公共字段，方便测试时查看当前目标
    [HideInInspector] public GameObject currentAimedObject;

    private Camera playerCamera;
    private RaycastHit lastHitInfo; // 保存最后一次命中信息

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
            Debug.Log("使用主摄像机作为瞄准摄像机");
        }

        if (crosshairUI == null)
        {
            Debug.LogWarning("准心UI未赋值，请在Inspector中关联。");
        }
        else
        {
            // 确保准心初始为默认颜色
            UpdateCrosshair(false);
        }
    }

    void Update()
    {
        PerformAimDetection();

        // 调试：在控制台显示当前状态
        Debug.Log($"瞄准状态: {(currentAimedObject != null ? $"已瞄准 {currentAimedObject.name}" : "无目标")}");
    }

    void PerformAimDetection()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerAiming: 未找到摄像机！");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxAimDistance, aimLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (currentAimedObject != hitObject)
            {
                currentAimedObject = hitObject;
                lastHitInfo = hit; // 保存命中信息
                UpdateCrosshair(true);
                OnAimTargetChanged?.Invoke(currentAimedObject);

                // 详细的调试信息
                Debug.Log($"<color=green>击中对象:</color> {hitObject.name}");
                Debug.Log($"<color=yellow>层级:</color> {LayerMask.LayerToName(hitObject.layer)}");
                Debug.Log($"<color=cyan>距离:</color> {hit.distance:F2}米");
            }
        }
        else
        {
            if (currentAimedObject != null)
            {
                currentAimedObject = null;
                UpdateCrosshair(false);
                OnAimTargetChanged?.Invoke(null);
            }
        }

        // 调试射线
        if (showDebugRay)
        {
            Color rayColor = (currentAimedObject != null) ? Color.green : debugRayColor;
            float rayLength = (currentAimedObject != null) ? lastHitInfo.distance : maxAimDistance;
            Debug.DrawRay(ray.origin, ray.direction * rayLength, rayColor);
        }
    }

    void UpdateCrosshair(bool canInteract)
    {
        if (crosshairUI != null)
        {
            var image = crosshairUI.GetComponent<Image>();
            if (image != null)
            {
                image.color = canInteract ? canInteractColor : defaultColor;
            }
        }
    }

    public GameObject GetCurrentAimedObject()
    {
        return currentAimedObject;
    }

    public bool GetAimHitInfo(out RaycastHit hitInfo)
    {
        hitInfo = lastHitInfo;
        return currentAimedObject != null;
    }

    // === 新增的测试方法 ===

    // 方法1：手动测试准心颜色切换
    public void ToggleCrosshairForTest()
    {
        if (crosshairUI != null)
        {
            var image = crosshairUI.GetComponent<Image>();
            if (image != null)
            {
                image.color = (image.color == defaultColor) ? canInteractColor : defaultColor;
                Debug.Log($"手动切换准心颜色: {image.color}");
            }
        }
    }

    // 方法2：获取当前射线信息
    public string GetAimDebugInfo()
    {
        if (currentAimedObject != null)
        {
            return $"目标: {currentAimedObject.name}\n" +
                   $"层级: {LayerMask.LayerToName(currentAimedObject.layer)}\n" +
                   $"距离: {lastHitInfo.distance:F2}米";
        }
        return "无瞄准目标";
    }

    // 方法3：临时更改最大距离（测试用）
    public void SetMaxDistanceForTest(float newDistance)
    {
        float oldDistance = maxAimDistance;
        maxAimDistance = newDistance;
        Debug.Log($"最大瞄准距离从 {oldDistance} 改为 {newDistance}");
    }
}