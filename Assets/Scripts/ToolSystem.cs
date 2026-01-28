using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ToolSystem : MonoBehaviour
{
    [System.Serializable]
    public class Tool
    {
        public string toolName;
        public GameObject toolPrefab;    // 工具模型预制体
        public Sprite toolIcon;          // UI图标
        public float damageRadius = 0.2f; // 破坏半径
        public float damageForce = 10f;  // 破坏力度
        public KeyCode hotkey;           // 快捷键
    }

    [Header("工具列表")]
    public List<Tool> tools = new List<Tool>();  // 改为public以便访问

    [Header("工具挂载点")]
    [SerializeField] private Transform toolHolder; // 工具在手中的挂载点

    [Header("调试设置")]
    [SerializeField] private bool debugMode = true;

    // 当前工具状态
    private int currentToolIndex = 0;
    private GameObject currentToolInstance;
    private Vector3 originalToolScale = Vector3.one;

    // 事件：工具切换时触发
    public delegate void ToolSwitchedHandler(Tool newTool);
    public event ToolSwitchedHandler OnToolSwitched;

    void Start()
    {
        InitializeToolSystem();

        // 初始调试信息
        if (debugMode)
        {
            Debug.Log("=== 工具系统初始化 ===");
            Debug.Log($"工具数量: {tools.Count}");
            Debug.Log($"当前工具索引: {currentToolIndex}");
        }
    }

    void Update()
    {
        HandleToolSwitching();
        HandleToolActions();
    }

    void InitializeToolSystem()
    {
        // 确保有主摄像机
        if (Camera.main == null)
        {
            Debug.LogError("❌ 场景中没有主摄像机！请添加Main Camera");
            return;
        }

        // 初始化工具挂载点
        if (toolHolder == null)
        {
            CreateToolHolder();
        }

        // 验证工具列表
        if (tools.Count == 0 && debugMode)
        {
            Debug.LogWarning("⚠️ 工具列表为空，请添加工具或使用'一键配置所有工具'");
        }

        // 装备第一个工具
        if (tools.Count > 0)
        {
            EquipTool(currentToolIndex);
        }
        else
        {
            Debug.LogError("❌ 没有可用的工具，无法初始化");
        }
    }

    void CreateToolHolder()
    {
        GameObject holderObj = new GameObject("ToolHolder");
        toolHolder = holderObj.transform;

        // 挂载到摄像机
        if (Camera.main != null)
        {
            toolHolder.SetParent(Camera.main.transform);
            // 调整位置：在摄像机右下角
            toolHolder.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            toolHolder.localRotation = Quaternion.Euler(10f, -10f, 0f);
        }
        else
        {
            // 如果没有摄像机，挂载到当前对象
            toolHolder.SetParent(transform);
            toolHolder.localPosition = Vector3.zero;
        }

        if (debugMode)
            Debug.Log($"✅ 创建工具挂载点: {toolHolder.name}");
    }

    void HandleToolSwitching()
    {
        // 鼠标滚轮切换工具
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            SwitchTool(direction);

            if (debugMode)
                Debug.Log($"鼠标滚轮切换: scroll={scroll}, direction={direction}");
        }

        // 数字键切换工具
        for (int i = 0; i < tools.Count; i++)
        {
            if (Input.GetKeyDown(tools[i].hotkey))
            {
                EquipTool(i);

                if (debugMode)
                    Debug.Log($"快捷键切换: 按下了 {tools[i].hotkey} -> 工具{i}");
                break;
            }
        }

        // Q键切换到上一个工具
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchTool(-1);

            if (debugMode)
                Debug.Log("Q键按下: 切换到上一个工具");
        }

        // Tab键切换到下一个工具
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchTool(1);

            if (debugMode)
                Debug.Log("Tab键按下: 切换到下一个工具");
        }
    }

    void HandleToolActions()
    {
        // 工具使用动作（比如挥动）
        if (Input.GetMouseButtonDown(0) && currentToolInstance != null) // 左键点击
        {
            PerformToolAction();
        }

        // 工具动画或效果
        if (currentToolInstance != null)
        {
            AnimateCurrentTool();
        }
    }

    void PerformToolAction()
    {
        // 播放工具使用动画
        if (currentToolInstance != null)
        {
            StartCoroutine(SwingToolAnimation());
        }

        // 触发工具使用事件
        Tool currentTool = GetCurrentTool();
        if (currentTool != null && debugMode)
        {
            Debug.Log($"使用工具: {currentTool.toolName}");
        }
    }

    System.Collections.IEnumerator SwingToolAnimation()
    {
        if (currentToolInstance == null) yield break;

        Transform toolTransform = currentToolInstance.transform;
        Vector3 originalRotation = toolTransform.localEulerAngles;

        // 向前摆动
        float swingAngle = 30f;
        float swingTime = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < swingTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / swingTime;

            // 摆动动画
            float angle = Mathf.Sin(t * Mathf.PI) * swingAngle;
            toolTransform.localEulerAngles = new Vector3(
                originalRotation.x + angle,
                originalRotation.y,
                originalRotation.z
            );

            yield return null;
        }

        // 恢复原位
        toolTransform.localEulerAngles = originalRotation;
    }

    void AnimateCurrentTool()
    {
        // 添加轻微的呼吸动画
        float breathSpeed = 2f;
        float breathAmount = 0.01f;

        float breathOffset = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
        Vector3 scale = originalToolScale * (1f + breathOffset);

        currentToolInstance.transform.localScale = scale;
    }

    public void SwitchTool(int direction)
    {
        int newIndex = currentToolIndex + direction;

        // 循环切换逻辑
        if (newIndex < 0)
            newIndex = tools.Count - 1;
        else if (newIndex >= tools.Count)
            newIndex = 0;

        EquipTool(newIndex);
    }

    public void EquipTool(int index)
    {
        // 验证索引
        if (index < 0 || index >= tools.Count)
        {
            Debug.LogError($"❌ 无效的工具索引: {index}，可用范围: 0-{tools.Count - 1}");
            return;
        }

        // 销毁当前工具实例
        if (currentToolInstance != null)
        {
            Destroy(currentToolInstance);
            currentToolInstance = null;
        }

        // 更新当前工具索引
        currentToolIndex = index;
        Tool newTool = tools[currentToolIndex];

        // 实例化新工具
        if (newTool.toolPrefab != null && toolHolder != null)
        {
            currentToolInstance = Instantiate(newTool.toolPrefab, toolHolder);
            currentToolInstance.transform.localPosition = Vector3.zero;
            currentToolInstance.transform.localRotation = Quaternion.identity;

            // 保存原始缩放
            originalToolScale = currentToolInstance.transform.localScale;

            // 设置工具标签（检查标签是否存在）
            string toolTag = "Tool";
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(toolTag))
            {
                // 标签不存在，创建它
#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.AddTag(toolTag);
#endif
                Debug.Log($"已创建标签: {toolTag}");
            }

            currentToolInstance.tag = toolTag;

            if (debugMode)
                Debug.Log($"✅ 装备工具: {newTool.toolName}");
        }
        else
        {
            Debug.LogError($"❌ 无法实例化工具 '{newTool.toolName}'，预制体或挂载点为空");

            // 创建临时替代品
            CreateTemporaryTool(newTool.toolName);
            return;
        }

        // 更新UI（不显示警告）
        UpdateToolUISilently(newTool.toolIcon, newTool.toolName);

        // 触发工具切换事件
        OnToolSwitched?.Invoke(newTool);

        // 调试信息
        if (debugMode)
        {
            Debug.Log($"🔄 工具切换完成: 索引={index}, 名称={newTool.toolName}");
        }
    }

    // 创建临时工具替代品
    void CreateTemporaryTool(string toolName)
    {
        GameObject tempTool;

        if (toolName.Contains("洛阳铲"))
        {
            tempTool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tempTool.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
        }
        else if (toolName.Contains("铁锹"))
        {
            tempTool = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempTool.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
        }
        else // 毛刷
        {
            tempTool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tempTool.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        }

        tempTool.transform.SetParent(toolHolder);
        tempTool.transform.localPosition = Vector3.zero;
        tempTool.transform.localRotation = Quaternion.identity;

        currentToolInstance = tempTool;
        originalToolScale = tempTool.transform.localScale;

        // 设置临时工具的标签
        currentToolInstance.tag = "Tool";
    }

    // 静默更新UI，不显示警告
    void UpdateToolUISilently(Sprite icon, string name)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateToolUI(icon, name);
        }
        // 不显示警告，只在debug模式下记录
        else if (debugMode)
        {
            Debug.Log($"切换工具到: {name} (UI系统未就绪)");
        }
    }

    // 获取当前工具
    public Tool GetCurrentTool()
    {
        if (currentToolIndex >= 0 && currentToolIndex < tools.Count)
            return tools[currentToolIndex];

        if (debugMode && tools.Count == 0)
            Debug.LogError("❌ 工具列表为空，无法获取当前工具");

        return null;
    }

    // 获取工具数量
    public int GetToolCount()
    {
        return tools.Count;
    }

    // 获取当前工具索引
    public int GetCurrentToolIndex()
    {
        return currentToolIndex;
    }

    // 获取当前工具的破坏参数
    public float GetCurrentDamageRadius()
    {
        Tool tool = GetCurrentTool();
        return tool != null ? tool.damageRadius : 0.2f;
    }

    public float GetCurrentDamageForce()
    {
        Tool tool = GetCurrentTool();
        return tool != null ? tool.damageForce : 10f;
    }

    // 获取当前工具实例（用于Rayfire交互）
    public GameObject GetCurrentToolInstance()
    {
        return currentToolInstance;
    }

    // ========== 编辑器辅助功能 ==========

    [ContextMenu("一键配置所有工具")]
    void OneClickConfigureAllTools()
    {
        Debug.Log("=== 开始一键配置所有工具 ===");

        // 1. 清除现有工具
        tools.Clear();

        // 2. 检查并创建缺失的预制体
#if UNITY_EDITOR
        CheckAndCreateMissingPrefabs();
#endif

        // 3. 加载预制体
        GameObject shovelPrefab = LoadPrefab("Test_洛阳铲");
        GameObject spadePrefab = LoadPrefab("Test_铁锹");
        GameObject brushPrefab = LoadPrefab("Test_毛刷");

        // 4. 配置工具列表
        if (shovelPrefab != null)
        {
            tools.Add(new Tool()
            {
                toolName = "洛阳铲",
                toolPrefab = shovelPrefab,
                damageRadius = 0.15f,
                damageForce = 8f,
                hotkey = KeyCode.Alpha1
            });
            Debug.Log("✅ 配置洛阳铲");
        }

        if (spadePrefab != null)
        {
            tools.Add(new Tool()
            {
                toolName = "铁锹",
                toolPrefab = spadePrefab,
                damageRadius = 0.25f,
                damageForce = 15f,
                hotkey = KeyCode.Alpha2
            });
            Debug.Log("✅ 配置铁锹");
        }

        if (brushPrefab != null)
        {
            tools.Add(new Tool()
            {
                toolName = "毛刷",
                toolPrefab = brushPrefab,
                damageRadius = 0.05f,
                damageForce = 2f,
                hotkey = KeyCode.Alpha3
            });
            Debug.Log("✅ 配置毛刷");
        }

        // 5. 如果没有找到预制体，创建临时工具
        if (tools.Count == 0)
        {
            Debug.LogWarning("⚠️ 未找到预制体，创建临时工具...");
            EmergencyCreateTools();
        }
        else
        {
            Debug.Log($"✅ 成功配置 {tools.Count} 个工具");

            // 装备第一个工具
            if (Application.isPlaying && tools.Count > 0)
            {
                EquipTool(0);
            }
        }

        Debug.Log("=== 一键配置完成 ===");
    }

    GameObject LoadPrefab(string prefabName)
    {
        // 尝试加载预制体
#if UNITY_EDITOR
        string path = $"Assets/Prefabs/Tools/{prefabName}.prefab";
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        // 运行时加载
        return Resources.Load<GameObject>($"Tools/{prefabName}");
#endif
    }

    [ContextMenu("紧急修复：创建临时工具")]
    void EmergencyCreateTools()
    {
        Debug.Log("紧急创建临时工具...");

        // 清除现有工具
        tools.Clear();

        // 创建临时洛阳铲（简单立方体）
        GameObject tempShovel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tempShovel.name = "Temp_洛阳铲";
        tempShovel.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
        tempShovel.transform.position = new Vector3(100, 100, 100); // 放到远处
        tempShovel.hideFlags = HideFlags.HideAndDontSave;

        // 创建临时铁锹
        GameObject tempSpade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempSpade.name = "Temp_铁锹";
        tempSpade.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
        tempSpade.transform.position = new Vector3(100, 100, 100);
        tempSpade.hideFlags = HideFlags.HideAndDontSave;

        // 创建临时毛刷
        GameObject tempBrush = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tempBrush.name = "Temp_毛刷";
        tempBrush.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        tempBrush.transform.position = new Vector3(100, 100, 100);
        tempBrush.hideFlags = HideFlags.HideAndDontSave;

        // 添加到工具列表
        tools.Add(new Tool()
        {
            toolName = "洛阳铲",
            toolPrefab = null, // 不使用预制体，运行时创建
            damageRadius = 0.15f,
            damageForce = 8f,
            hotkey = KeyCode.Alpha1
        });

        tools.Add(new Tool()
        {
            toolName = "铁锹",
            toolPrefab = null,
            damageRadius = 0.25f,
            damageForce = 15f,
            hotkey = KeyCode.Alpha2
        });

        tools.Add(new Tool()
        {
            toolName = "毛刷",
            toolPrefab = null,
            damageRadius = 0.05f,
            damageForce = 2f,
            hotkey = KeyCode.Alpha3
        });

        Debug.Log("✅ 紧急修复完成，现在有3个临时工具");

        // 立即装备第一个工具
        if (Application.isPlaying)
        {
            EquipTool(0);
        }
    }

#if UNITY_EDITOR
    void CheckAndCreateMissingPrefabs()
    {
        // 确保目录存在
        if (!System.IO.Directory.Exists("Assets/Prefabs/Tools"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/Tools");

        // 检查洛阳铲
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tools/Test_洛阳铲.prefab") == null)
        {
            Debug.Log("创建洛阳铲预制体...");
            CreateSimplePrefab("Test_洛阳铲", PrimitiveType.Cylinder, new Vector3(0.1f, 0.5f, 0.1f));
        }

        // 检查铁锹
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tools/Test_铁锹.prefab") == null)
        {
            Debug.Log("创建铁锹预制体...");
            CreateSimplePrefab("Test_铁锹", PrimitiveType.Cube, new Vector3(0.3f, 0.05f, 0.2f));
        }

        // 检查毛刷
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tools/Test_毛刷.prefab") == null)
        {
            Debug.Log("创建毛刷预制体...");
            CreateSimplePrefab("Test_毛刷", PrimitiveType.Cylinder, new Vector3(0.05f, 0.2f, 0.05f));
        }

        // 刷新资产数据库
        UnityEditor.AssetDatabase.Refresh();
    }

    void CreateSimplePrefab(string name, PrimitiveType type, Vector3 scale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.localScale = scale;

        // 使用sharedMaterial避免材质泄漏警告
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetToolColor(name);
            renderer.sharedMaterial = mat;
        }

        string path = $"Assets/Prefabs/Tools/{name}.prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path);

        // 延迟销毁，避免立即销毁问题
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (obj != null)
                DestroyImmediate(obj);
        };

        Debug.Log($"创建预制体: {path}");
    }

    Color GetToolColor(string toolName)
    {
        if (toolName.Contains("洛阳铲")) return new Color(0.6f, 0.6f, 0.6f); // 灰色
        if (toolName.Contains("铁锹")) return new Color(0.5f, 0.3f, 0.1f); // 棕色
        if (toolName.Contains("毛刷")) return new Color(0.9f, 0.8f, 0.6f); // 浅黄色
        return Color.white;
    }
#endif

    [ContextMenu("打印当前状态")]
    void PrintCurrentStatus()
    {
        Debug.Log("=== 工具系统当前状态 ===");
        Debug.Log($"工具总数: {tools.Count}");
        Debug.Log($"当前工具索引: {currentToolIndex}");

        Tool current = GetCurrentTool();
        if (current != null)
        {
            Debug.Log($"当前工具名称: {current.toolName}");
            Debug.Log($"当前工具预制体: {current.toolPrefab}");
            Debug.Log($"当前工具快捷键: {current.hotkey}");
        }
        else
        {
            Debug.LogError("当前工具为空");
        }

        Debug.Log($"工具实例是否存在: {currentToolInstance != null}");
        Debug.Log($"工具挂载点: {toolHolder}");
        Debug.Log("========================");
    }

    // 简单的GUI显示
    void OnGUI()
    {
        if (!debugMode || !Application.isPlaying) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;

        Tool current = GetCurrentTool();
        if (current != null)
        {
            string toolInfo = $"当前工具: {current.toolName} (按{current.hotkey})";

            // 右上角显示工具信息
            float width = 300;
            float x = Screen.width - width - 10;
            float y = 10;

            GUI.Label(new Rect(x, y, width, 30), toolInfo, style);
            GUI.Label(new Rect(x, y + 25, width, 30), "鼠标滚轮切换 | Q键上一个工具", style);
        }
    }
}