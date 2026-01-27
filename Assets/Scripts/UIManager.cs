using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI元素 - 工具信息（右上角）")]
    [SerializeField] private Text toolDisplayText;
    [SerializeField] private Image toolIconImage;

    [Header("UI元素 - 玩家状态（左上角）")]
    [SerializeField] private Text playerStatusText;

    [Header("UI布局设置")]
    [SerializeField] private Color playerStatusColor = Color.cyan;
    [SerializeField] private Color toolStatusColor = Color.yellow;
    [SerializeField] private int playerFontSize = 16;
    [SerializeField] private int toolFontSize = 18;

    [Header("自动更新")]
    [SerializeField] private bool autoUpdatePlayerStatus = true;
    [SerializeField] private float updateInterval = 0.2f;

    private float updateTimer = 0f;
    private PlayerController playerController;
    private ToolSystem toolSystem;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 自动创建UI如果不存在
        CreateUIElementsIfNeeded();

        Debug.Log("✅ UIManager初始化完成");
    }

    void Start()
    {
        // 查找引用
        playerController = FindObjectOfType<PlayerController>();
        toolSystem = FindObjectOfType<ToolSystem>();

        // 初始更新
        UpdateAllUI();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            if (autoUpdatePlayerStatus)
            {
                UpdatePlayerStatusUI();
            }
            updateTimer = 0f;
        }

        // 快捷键：F1显示/隐藏调试信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugInfo();
        }
    }

    void CreateUIElementsIfNeeded()
    {
        // 如果Canvas不存在，创建它
        if (GetComponentInChildren<Canvas>() == null)
        {
            CreateCanvas();
        }

        // 确保必要的UI元素存在
        EnsureUIElements();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("✅ 创建Canvas");
    }

    void EnsureUIElements()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        // === 玩家状态文本（左上角） ===
        if (playerStatusText == null)
        {
            GameObject playerTextObj = new GameObject("PlayerStatus");
            playerTextObj.transform.SetParent(canvas.transform);

            playerStatusText = playerTextObj.AddComponent<Text>();
            playerStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerStatusText.fontSize = playerFontSize;
            playerStatusText.color = playerStatusColor;
            playerStatusText.alignment = TextAnchor.UpperLeft;

            RectTransform playerRect = playerTextObj.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0, 1);
            playerRect.anchorMax = new Vector2(0, 1);
            playerRect.pivot = new Vector2(0, 1);
            playerRect.anchoredPosition = new Vector2(10, -10);
            playerRect.sizeDelta = new Vector2(250, 120);

            playerStatusText.text = "玩家状态: 初始化中...";

            Debug.Log("✅ 创建玩家状态UI");
        }

        // === 工具信息文本（右上角） ===
        if (toolDisplayText == null)
        {
            GameObject toolTextObj = new GameObject("ToolStatus");
            toolTextObj.transform.SetParent(canvas.transform);

            toolDisplayText = toolTextObj.AddComponent<Text>();
            toolDisplayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            toolDisplayText.fontSize = toolFontSize;
            toolDisplayText.color = toolStatusColor;
            toolDisplayText.alignment = TextAnchor.UpperRight;

            RectTransform toolRect = toolTextObj.GetComponent<RectTransform>();
            toolRect.anchorMin = new Vector2(1, 1);
            toolRect.anchorMax = new Vector2(1, 1);
            toolRect.pivot = new Vector2(1, 1);
            toolRect.anchoredPosition = new Vector2(-10, -10);
            toolRect.sizeDelta = new Vector2(300, 100);

            toolDisplayText.text = "当前工具: 无\n快捷键: 1/2/3";

            Debug.Log("✅ 创建工具状态UI");
        }

        // === 工具图标（可选）===
        if (toolIconImage == null)
        {
            GameObject iconObj = new GameObject("ToolIcon");
            iconObj.transform.SetParent(canvas.transform);

            toolIconImage = iconObj.AddComponent<Image>();
            toolIconImage.color = new Color(1, 1, 1, 0.8f);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1, 1);
            iconRect.anchorMax = new Vector2(1, 1);
            iconRect.pivot = new Vector2(1, 1);
            iconRect.anchoredPosition = new Vector2(-150, -120);
            iconRect.sizeDelta = new Vector2(64, 64);

            Debug.Log("✅ 创建工具图标UI");
        }
    }

    // === 公共方法 ===

    public void UpdateToolUI(Sprite icon, string name)
    {
        if (toolDisplayText != null)
        {
            toolDisplayText.text = $"当前工具: {name}\n" +
                                 $"快捷键: 1/2/3\n" +
                                 $"滚轮/Q键切换";
        }

        if (toolIconImage != null && icon != null)
        {
            toolIconImage.sprite = icon;
            toolIconImage.color = Color.white;
        }
        else if (toolIconImage != null)
        {
            // 如果没有图标，隐藏图像
            toolIconImage.color = new Color(1, 1, 1, 0);
        }

        Debug.Log($"UI更新: 当前工具 - {name}");
    }

    public void UpdatePlayerStatus(string status, float speed, string additionalInfo = "")
    {
        if (playerStatusText != null)
        {
            string statusText = $"玩家状态: {status}\n" +
                              $"速度: {speed:F1}\n" +
                              $"WASD: 移动\n" +
                              $"空格: 跳跃\n" +
                              $"Shift: 冲刺\n" +
                              $"Ctrl: 蹲下";

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                statusText += $"\n{additionalInfo}";
            }

            playerStatusText.text = statusText;
        }
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        // 创建临时消息
        StartCoroutine(ShowTemporaryMessage(message, duration));
    }

    System.Collections.IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        // 创建临时消息对象
        GameObject messageObj = new GameObject("TempMessage");
        messageObj.transform.SetParent(GetComponentInChildren<Canvas>().transform);

        Text messageText = messageObj.AddComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 20;
        messageText.color = Color.white;
        messageText.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = messageObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(500, 50);

        messageText.text = message;

        // 淡入效果
        float fadeInTime = 0.3f;
        for (float t = 0; t < fadeInTime; t += Time.deltaTime)
        {
            messageText.color = new Color(1, 1, 1, t / fadeInTime);
            yield return null;
        }

        messageText.color = Color.white;

        // 等待
        yield return new WaitForSeconds(duration);

        // 淡出效果
        float fadeOutTime = 0.3f;
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            messageText.color = new Color(1, 1, 1, 1 - (t / fadeOutTime));
            yield return null;
        }

        Destroy(messageObj);
    }

    // === 私有方法 ===

    void UpdatePlayerStatusUI()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null) return;
        }

        string status = "站立";
        if (playerController.isCrouching) status = "蹲下";
        if (playerController.isSprinting) status = "冲刺";

        float speed = playerController.currentSpeed;

        UpdatePlayerStatus(status, speed);
    }

    void UpdateAllUI()
    {
        UpdatePlayerStatusUI();

        // 更新工具UI
        if (toolSystem != null)
        {
            var currentTool = toolSystem.GetCurrentTool();
            if (currentTool != null)
            {
                UpdateToolUI(currentTool.toolIcon, currentTool.toolName);
            }
        }
    }

    void ToggleDebugInfo()
    {
        // 切换调试信息显示
        if (playerStatusText != null)
        {
            bool isActive = playerStatusText.gameObject.activeSelf;
            playerStatusText.gameObject.SetActive(!isActive);
            toolDisplayText.gameObject.SetActive(!isActive);

            Debug.Log($"UI显示: {!isActive}");
        }
    }

    // === 编辑器辅助 ===

#if UNITY_EDITOR
    [ContextMenu("创建/重置UI")]
    void CreateResetUI()
    {
        // 删除现有UI
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Canvas") || child.name.Contains("UI"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        // 重新创建
        CreateUIElementsIfNeeded();
        
        Debug.Log("✅ UI已重置");
    }
    
    [ContextMenu("测试UI更新")]
    void TestUIUpdate()
    {
        UpdateToolUI(null, "测试工具");
        UpdatePlayerStatus("测试状态", 5.5f, "测试信息");
        ShowMessage("UI测试消息", 3f);
    }
#endif
}