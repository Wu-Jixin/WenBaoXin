using UnityEngine;
using LibreFracture;

public class ToolHandler : MonoBehaviour
{
    [System.Serializable]
    public class ToolSettings
    {
        public string toolName = "Tool";
        public float attackRange = 2f;
        public float attackCooldown = 0.5f;
        public KeyCode attackKey = KeyCode.Mouse1;
    }

    [Header("工具设置")]
    public ToolSettings settings;

    [Header("引用")]
    public Camera playerCamera;

    private bool canAttack = true;
    private float lastAttackTime;

    private ToolItem toolItem;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        toolItem = GetComponent<ToolItem>();

        Debug.Log("ToolHandler 初始化：" + settings.toolName);
    }

    //void Update()
    //{
    //if (Input.GetKeyDown(settings.attackKey))
    //{
    //TriggerAttack();
    //}

    // HandleCooldown();
    //}

    void Update()
    {
        HandleCooldown(); // 只保留冷却
    }

    void HandleCooldown()
    {
        if (!canAttack && Time.time - lastAttackTime > settings.attackCooldown)
        {
            canAttack = true;
        }
    }

    public void TriggerAttack()
    {
        if (!canAttack) return;

        canAttack = false;
        lastAttackTime = Time.time;

        ToolType type = ToolType.None;

        if (toolItem != null)
            type = toolItem.GetToolType();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, settings.attackRange))
        {
            Debug.Log("命中物体：" + hit.collider.name);

            HandleToolAction(type, hit);
        }
        else
        {
            Debug.Log("未命中物体");
        }
    }

    // ================================
    // 工具行为控制
    // ================================

    void HandleToolAction(ToolType type, RaycastHit hit)
    {
        switch (type)
        {
            case ToolType.Brush:
                UseBrush(hit);
                break;

            case ToolType.Shovel:
                UseShovel(hit);
                break;

            case ToolType.Spade:
                UseSpade(hit);
                break;

            case ToolType.Tweezers:
                UseTweezers(hit);
                break;

            case ToolType.Glue:
                UseGlue(hit);
                break;

            case ToolType.Scanner:
                UseScanner(hit);
                break;

            case ToolType.Marker:
                UseMarker(hit);
                break;

            case ToolType.Probe:
                UseProbe(hit);
                break;

            default:
                Debug.Log("未知工具");
                break;
        }
    }

    // ================================
    // 毛刷
    // ================================

    void UseBrush(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Soil"))
        {
            Debug.Log("毛刷正在清理土层");

            Renderer r = hit.collider.GetComponent<Renderer>();

            if (r != null)
            {
                Color c = r.material.color;
                c.a -= 0.2f;
                r.material.color = c;

                if (c.a <= 0.1f)
                {
                    Destroy(hit.collider.gameObject);
                    Debug.Log("土层清理完成");
                }
            }
        }
        else
        {
            Debug.Log("毛刷只能清理土层");
        }
    }

    // ================================
    // 洛阳铲
    // ================================

    void UseShovel(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Soil"))
        {
            Debug.Log("洛阳铲挖掘土层");

            BreakObject(hit);
        }
        else
        {
            Debug.Log("洛阳铲只能挖土");
        }
    }

    // ================================
    // 铁锹
    // ================================

    void UseSpade(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Breakable"))
        {
            Debug.Log("铁锹破坏墙体");

            BreakObject(hit);
        }
        else
        {
            Debug.Log("铁锹对该物体无效");
        }
    }

    // ================================
    // 镊子
    // ================================

    void UseTweezers(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Artifact"))
        {
            Debug.Log("使用镊子拾取文物");

            hit.collider.gameObject.SetActive(false);

            if (VisualFeedbackUI.Instance != null)
                VisualFeedbackUI.Instance.ShowHitFeedback();
        }
        else
        {
            Debug.Log("镊子只能拾取文物");
        }
    }

    // ================================
    // 修复胶
    // ================================

    void UseGlue(RaycastHit hit)
    {
        if (hit.collider.CompareTag("BrokenArtifact"))
        {
            Debug.Log("修复文物");

            hit.collider.tag = "Artifact";

            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r != null)
                r.material.color = Color.white;
        }
        else
        {
            Debug.Log("没有需要修复的文物");
        }
    }

    // ================================
    // 扫描仪
    // ================================

    void UseScanner(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Artifact"))
        {
            Debug.Log("扫描文物信息");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGuidance(
                    "扫描完成\n文物：大克鼎\n时期：西周中期"
                );
            }
        }
        else
        {
            Debug.Log("扫描未发现文物");
        }
    }

    // ================================
    // LibreFracture破坏
    // ================================

    void BreakObject(RaycastHit hit)
    {
        ChunkGraphManager manager = hit.collider.GetComponentInParent<ChunkGraphManager>();

        if (manager != null)
        {
            ChunkNode[] chunks = manager.GetComponentsInChildren<ChunkNode>();

            foreach (ChunkNode chunk in chunks)
            {
                Rigidbody rb = chunk.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 dir = (chunk.transform.position - hit.point).normalized;

                    rb.isKinematic = false;
                    rb.useGravity = true;

                    rb.AddForce(dir * 15f + Vector3.up * 8f, ForceMode.Impulse);
                }
            }

            manager.gameObject.SetActive(false);

            Debug.Log("破碎完成");
        }
    }

    void UseMarker(RaycastHit hit)
    {
        if (hit.collider.CompareTag("TopSoil"))
        {
            Debug.Log("划线工具触发（实际由MarkingSystem执行）");
        }
    }

    void UseProbe(RaycastHit hit)
    {
        if (hit.collider.CompareTag("TopSoil"))
        {
            Debug.Log("探孔工具触发（实际由ProbeSystem执行）");
        }
    }
}