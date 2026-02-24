using UnityEngine;
using LibreFracture;  // 添加LibreFracture命名空间

public class ToolHandler : MonoBehaviour
{
    [System.Serializable]
    public class ToolSettings
    {
        public string toolName = "洛阳铲";
        public float damage = 30f;
        public float attackRange = 2f;
        public float attackCooldown = 0.5f;
        public KeyCode attackKey = KeyCode.Mouse0;
    }

    public ToolSettings settings;
    public Transform toolTransform;
    public Camera playerCamera;

    private bool canAttack = true;
    private float lastAttackTime;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        Debug.Log($"ToolHandler 初始化: {settings.toolName}");
    }

    void Update()
    {
        // 冷却检查
        if (!canAttack && Time.time - lastAttackTime > settings.attackCooldown)
        {
            canAttack = true;
            Debug.Log($"工具 {settings.toolName} 冷却结束");
        }
    }

    // 这个方法会被 ToolSystem 调用
    public void TriggerAttack()
    {
        Debug.Log($"TriggerAttack 被调用, canAttack={canAttack}");

        if (!canAttack)
        {
            Debug.Log("工具冷却中");
            if (VisualFeedbackUI.Instance != null)
                VisualFeedbackUI.Instance.ShowCooldownFeedback(GetRemainingCooldown());
            return;
        }

        canAttack = false;
        lastAttackTime = Time.time;

        Debug.Log($"执行攻击: {settings.toolName}, 伤害={settings.damage}");

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, settings.attackRange))
        {
            // 修复：使用 hit.collider.gameObject.layer 而不是 hit.collider.layer
            Debug.Log($"射线击中: {hit.collider.name}, 标签: {hit.collider.tag}, 层级: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            // === 破碎核心逻辑 ===

            // 1. 先找 ChunkGraphManager（主物体）
            ChunkGraphManager manager = hit.collider.GetComponentInParent<ChunkGraphManager>();
            if (manager != null)
            {
                Debug.Log($"找到 ChunkGraphManager: {manager.name}");

                // 2. 找到所有 ChunkNode
                ChunkNode[] chunks = manager.GetComponentsInChildren<ChunkNode>();
                Debug.Log($"找到 {chunks.Length} 个碎片");

                // 3. 给所有碎片施加力（模拟破碎）
                foreach (ChunkNode chunk in chunks)
                {
                    Rigidbody rb = chunk.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 让碎片从击中点向外飞散
                        Vector3 forceDir = (chunk.transform.position - hit.point).normalized;
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.AddForce(forceDir * 15f + Vector3.up * 8f, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.Impulse);

                        Debug.Log($"给碎片 {chunk.name} 施加力: {forceDir}");

                        // 断开连接（如果有 Joint）
                        Joint joint = chunk.GetComponent<Joint>();
                        if (joint != null)
                        {
                            Destroy(joint);
                            Debug.Log($"销毁 Joint: {joint.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"碎片 {chunk.name} 没有 Rigidbody");
                    }
                }

                // 4. 隐藏主物体
                manager.gameObject.SetActive(false);
                Debug.Log("主物体已隐藏");

                // 5. 触发教育报告
                if (LossReportSystem.Instance != null)
                {
                    LossReportSystem.Instance.ShowReport("Tomb_Wall");
                    Debug.Log("触发教育报告");
                }
            }
            else
            {
                Debug.Log("未找到 ChunkGraphManager，尝试找单个 ChunkNode");

                // 尝试找单个 ChunkNode（如果是直接攻击碎片）
                ChunkNode node = hit.collider.GetComponent<ChunkNode>();
                if (node != null)
                {
                    Debug.Log($"找到 ChunkNode: {node.name}");
                    Rigidbody rb = node.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 forceDir = (node.transform.position - hit.point).normalized;
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.AddForce(forceDir * 15f + Vector3.up * 8f, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.Impulse);

                        // 断开连接
                        Joint joint = node.GetComponent<Joint>();
                        if (joint != null) Destroy(joint);
                    }
                }
            }

            // 视觉反馈
            if (VisualFeedbackUI.Instance != null)
            {
                VisualFeedbackUI.Instance.ShowHitFeedback();
                VisualFeedbackUI.Instance.ShakeCamera(1f);
                VisualFeedbackUI.Instance.ShowDamageNumber(hit.point, settings.damage);
                Debug.Log("触发视觉反馈");
            }
        }
        else
        {
            Debug.Log("射线未击中任何物体");

            if (VisualFeedbackUI.Instance != null)
            {
                VisualFeedbackUI.Instance.ShowMissFeedback();
                Debug.Log("触发未命中反馈");
            }
        }
    }

    // 手动触发攻击（用于测试）
    public void TestAttack()
    {
        Debug.Log("测试攻击");
        TriggerAttack();
    }

    public bool CanAttack() => canAttack;

    public string GetToolName() => settings.toolName;

    public float GetRemainingCooldown()
    {
        if (canAttack) return 0f;
        float remaining = settings.attackCooldown - (Time.time - lastAttackTime);
        return Mathf.Max(0f, remaining);
    }

    // 重置冷却（用于调试）
    public void ResetCooldown()
    {
        canAttack = true;
        lastAttackTime = 0f;
        Debug.Log("冷却已重置");
    }
}