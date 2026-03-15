using UnityEngine;
using UnityEngine.UI;

public class PlayerAiming : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;

    [Header("Crosshair UI")]
    public Image crosshairImage;

    [Header("Distance")]
    public float interactDistance = 5f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color interactColor = Color.green;
    public Color breakableColor = Color.red;
    public Color scannableColor = Color.cyan;

    [Header("Scan Settings")]
    public bool enableScanning = true;          // 是否启用扫描功能
    public KeyCode interactKey = KeyCode.E;     // 交互按键

    private GameObject currentTarget;
    private Scannable currentScannable;          // 当前扫描到的文物

    void Start()
    {
        // 自动寻找主相机
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 自动寻找准心
        if (crosshairImage == null)
        {
            GameObject crosshairObj = GameObject.Find("Crosshair");

            if (crosshairObj != null)
            {
                crosshairImage = crosshairObj.GetComponent<Image>();
            }
        }

        if (crosshairImage == null)
        {
            Debug.LogError("Crosshair Image 未找到！");
        }

        // 检查GuidanceManager是否存在
        if (enableScanning)
        {
            CheckGuidanceManager();
        }
    }

    void Update()
    {
        if (playerCamera == null || crosshairImage == null)
            return;

        DetectTarget();

        // 如果启用扫描，处理扫描逻辑
        if (enableScanning)
        {
            HandleScanning();
        }
    }

    void DetectTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            currentTarget = hit.collider.gameObject;

            // Scannable 优先
            if (currentTarget.GetComponent<Scannable>() != null)
            {
                crosshairImage.color = scannableColor;
            }
            // Breakable
            else if (currentTarget.GetComponent<Breakable>() != null)
            {
                crosshairImage.color = breakableColor;
            }
            // Interactable
            else if (currentTarget.GetComponent<Interactable>() != null)
            {
                crosshairImage.color = interactColor;
            }
            else
            {
                crosshairImage.color = normalColor;
            }
        }
        else
        {
            currentTarget = null;
            crosshairImage.color = normalColor;
        }
    }

    // ========== 新增：扫描处理逻辑 ==========
    void HandleScanning()
    {
        // 从屏幕中心发射射线
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // 获取Scannable组件
            Scannable scannable = hit.collider.GetComponent<Scannable>();

            if (scannable != null)
            {
                // 保存当前扫描对象
                currentScannable = scannable;

                // 构建指引信息
                string msg = $"发现文物：{scannable.artifactName}\n" +
                             $"{scannable.guidanceText}\n" +
                             $"[{interactKey}] 交互";

                // 显示指引
                if (GuidanceManager.Instance != null)
                {
                    GuidanceManager.Instance.ShowGuidance(msg);
                }
                else
                {
                    Debug.LogWarning("GuidanceManager.Instance 为 null，无法显示指引");
                }

                // 处理交互按键
                if (Input.GetKeyDown(interactKey))
                {
                    OnInteractWithScannable(scannable);
                }

                return; // 找到Scannable后直接返回
            }
        }

        // 没有扫描到任何东西，清除当前扫描对象并隐藏指引
        if (currentScannable != null)
        {
            currentScannable = null;
        }

        if (GuidanceManager.Instance != null)
        {
            GuidanceManager.Instance.HideGuidance();
        }
    }

    // ========== 新增：与文物交互 ==========
    void OnInteractWithScannable(Scannable scannable)
    {
        Debug.Log($"交互文物：{scannable.artifactName}");

        // 这里可以触发各种交互效果
        // 例如：播放音效、显示更多信息、触发任务进度等

        // 触发文物上的交互事件（如果有实现）
        Interactable interactable = scannable.GetComponent<Interactable>();
        if (interactable != null)
        {
           // interactable.OnInteract();
        }

        // 可以在这里添加更多交互逻辑
    }

    // ========== 新增：检查GuidanceManager ==========
    void CheckGuidanceManager()
    {
        if (GuidanceManager.Instance == null)
        {
            Debug.LogWarning("GuidanceManager.Instance 不存在，请确保场景中有 GuidanceManager");

            // 尝试查找
            GuidanceManager manager = FindObjectOfType<GuidanceManager>();
            if (manager != null)
            {
                Debug.Log("找到 GuidanceManager，但需要通过 Instance 访问，请检查 GuidanceManager 的单例实现");
            }
        }
    }

    // ========== 原有的公共方法 ==========
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    // ========== 新增：获取当前扫描的文物 ==========
    public Scannable GetCurrentScannable()
    {
        return currentScannable;
    }

    // ========== 新增：手动触发扫描 ==========
    public void ForceScan()
    {
        HandleScanning();
    }
}