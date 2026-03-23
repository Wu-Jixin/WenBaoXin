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
    public Color diggableColor = new Color(1f, 0.5f, 0f); // 橙色（土层）

    [Header("Scan Settings")]
    public bool enableScanning = true;
    public KeyCode interactKey = KeyCode.E;

    private GameObject currentTarget;
    private Scannable currentScannable;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (crosshairImage == null)
        {
            GameObject crosshairObj = GameObject.Find("Crosshair");
            if (crosshairObj != null)
                crosshairImage = crosshairObj.GetComponent<Image>();
        }

        if (crosshairImage == null)
            Debug.LogError("Crosshair Image 未找到！");

        if (enableScanning)
            CheckGuidanceManager();
    }

    void Update()
    {
        if (playerCamera == null || crosshairImage == null)
            return;

        DetectTarget();

        if (enableScanning)
            HandleScanning();

        HandleDigging(); // ⭐新增
    }

    // ================== 检测 ==================
    void DetectTarget()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            currentTarget = hit.collider.gameObject;

            // ⭐优先级排序
            if (currentTarget.GetComponent<Scannable>() != null)
            {
                crosshairImage.color = scannableColor;
            }
            else if (currentTarget.GetComponent<SoilLayer>() != null)
            {
                crosshairImage.color = diggableColor;
            }
            else if (currentTarget.GetComponent<Breakable>() != null)
            {
                crosshairImage.color = breakableColor;
            }
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

    // ================== ⭐挖掘系统 ==================
    void HandleDigging()
    {
        if (currentTarget == null) return;

        if (Input.GetMouseButtonDown(0)) // 左键挖
        {
            SoilLayer layer = currentTarget.GetComponent<SoilLayer>();

            if (layer != null)
            {
                layer.OnDig();
            }
        }
    }

    // ================== 扫描系统 ==================
    void HandleScanning()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            Scannable scannable = hit.collider.GetComponent<Scannable>();

            if (scannable != null)
            {
                currentScannable = scannable;

                string msg = $"发现文物：{scannable.artifactName}\n" +
                             $"{scannable.guidanceText}\n" +
                             $"[{interactKey}] 交互";

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowGuidance(msg);

                if (Input.GetKeyDown(interactKey))
                {
                    OnInteractWithScannable(scannable);
                }

                return;
            }
        }

        if (currentScannable != null)
            currentScannable = null;

        //if (UIManager.Instance != null)
            //UIManager.Instance.HideGuidance();
    }

    void OnInteractWithScannable(Scannable scannable)
    {
        Debug.Log($"交互文物：{scannable.artifactName}");

        Interactable interactable = scannable.GetComponent<Interactable>();
        if (interactable != null)
        {
            // interactable.OnInteract();
        }
    }

    void CheckGuidanceManager()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("UIManager.Instance 不存在");
        }
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    public Scannable GetCurrentScannable()
    {
        return currentScannable;
    }
}