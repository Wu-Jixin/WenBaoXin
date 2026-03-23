using System.Collections;
using UnityEngine;

public class SoilLayer : MonoBehaviour
{
    // ================= 土层类型枚举 =================
    public enum SoilType
    {
        TopSoil,    // 表层（铲子）
        SoftSoil,   // 松土（铲子/刷子）
        HardSoil,   // 硬土（铲子）
        Fine,       // 精细层（刷子）
        Artifact    // 文物层（扫描仪）
    }

    [Header("层级信息")]
    public int layerIndex;
    public bool isUnlocked = false;

    [Header("土层类型")]
    public SoilType soilType = SoilType.TopSoil;

    [Header("层级连接")]
    public SoilLayer nextLayer;

    [Header("设置")]
    public float removeDelay = 0.3f;   // 延迟删除时间
    public float playerLift = 0.15f;   // 防掉落抬高值

    private Collider col;
    private bool isDigging = false;     // 防止重复挖掘

    void Start()
    {
        col = GetComponent<Collider>();

        // 初始化碰撞状态
        if (col != null)
        {
            col.enabled = isUnlocked;
        }
    }

    // ================= 工具判断核心方法 =================
    public bool CanUseTool(ToolSystem.ToolType tool)
    {
        switch (soilType)
        {
            case SoilType.TopSoil:
                return tool == ToolSystem.ToolType.Shovel;

            case SoilType.SoftSoil:
                return tool == ToolSystem.ToolType.Shovel
                    || tool == ToolSystem.ToolType.Brush;

            case SoilType.HardSoil:
                return tool == ToolSystem.ToolType.Shovel;

            case SoilType.Fine:
                return tool == ToolSystem.ToolType.Brush;

            case SoilType.Artifact:
                return tool == ToolSystem.ToolType.Scanner;

            default:
                return false;
        }
    }

    // ================= 获取推荐工具提示 =================
    public string GetRecommendedToolTip()
    {
        switch (soilType)
        {
            case SoilType.TopSoil:
                return "需要【洛阳铲】或【铁锹】挖掘";
            case SoilType.SoftSoil:
                return "可使用【洛阳铲/铁锹】或【毛刷】";
            case SoilType.HardSoil:
                return "需要【洛阳铲】或【铁锹】";
            case SoilType.Fine:
                return "需要【毛刷】精细清理";
            case SoilType.Artifact:
                return "需要【扫描仪】探测";
            default:
                return "未知土层类型";
        }
    }

    // ================= 挖掘入口（由ToolSystem调用） =================
    public void OnDig()
    {
        // 防止重复挖掘
        if (isDigging)
        {
            return;
        }

        // 1️⃣ 未解锁检查
        if (!isUnlocked)
        {
            ShowGuidance("⚠️ 该土层尚未解锁，请先挖掘上层", 5f);
            return;
        }

        // 2️⃣ 获取当前工具
        ToolSystem toolSystem = FindObjectOfType<ToolSystem>();

        if (toolSystem == null)
        {
            Debug.LogError("❌ 找不到 ToolSystem");
            return;
        }

        ToolSystem.ToolType currentTool = toolSystem.GetCurrentToolType();

        // 3️⃣ 工具类型检查
        if (!CanUseTool(currentTool))
        {
            // 显示具体提示
            string tip = $"❌ {GetRecommendedToolTip()}";
            ShowGuidance(tip, 5f);

            // 播放错误音效（可选）
            PlaySound(false);
            return;
        }

        // 4️⃣ 成功挖掘
        Debug.Log($"✅ 挖掘土层 [{layerIndex}]：{gameObject.name}，使用工具：{currentTool}");

        // 播放成功音效（可选）
        PlaySound(true);

        // 开始挖掘协程
        StartCoroutine(DigRoutine());
    }

    // ================= 挖掘协程 =================
    IEnumerator DigRoutine()
    {
        isDigging = true;

        // ⭐1. 先解锁下一层（关键！）
        if (nextLayer != null)
        {
            nextLayer.Unlock();
            Debug.Log($"🔓 解锁下一层：{nextLayer.gameObject.name}");
        }

        // ⭐2. 抬高玩家（防止卡入/掉落）
        LiftPlayer();

        // ⭐3. 播放挖掘特效（可选）
        PlayDigEffect();

        // ⭐4. 延迟删除当前层
        yield return new WaitForSeconds(removeDelay);

        // 禁用而不是销毁，方便后续重置
        gameObject.SetActive(false);

        isDigging = false;

        Debug.Log($"🗑️ 土层已移除：{gameObject.name}");
    }

    // ================= 解锁土层 =================
    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        if (col == null)
            col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true; // ⭐开启碰撞（接住玩家）
        }

        Debug.Log($"🔓 解锁土层：{gameObject.name} (类型：{soilType})");

        // 播放解锁特效（可选）
        PlayUnlockEffect();
    }

    // ================= 防掉落 =================
    void LiftPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            player.transform.position += Vector3.up * playerLift;
            Debug.Log($"⬆️ 抬高玩家 {playerLift} 单位");
        }
    }

    // ================= UI提示（使用 GuidanceManager） =================
    void ShowGuidance(string message, float duration)
    {
        // 尝试使用 GuidanceManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGuidance(message, duration);
        }
        else
        {
            // 备用方案：输出到控制台
            Debug.Log($"[提示] {message}");
        }
    }

    // ================= 音效（可选，需要 AudioSource） =================
    void PlaySound(bool success)
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            // 如果有音效组件，可以播放不同音效
            // audio.PlayOneShot(success ? successClip : failClip);
        }
    }

    // ================= 特效（可选） =================
    void PlayDigEffect()
    {
        // 可以在这里添加粒子特效
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }

    void PlayUnlockEffect()
    {
        // 解锁时的视觉效果
        // 例如：材质闪烁、粒子等
    }

    // ================= 调试辅助 =================
    [ContextMenu("测试挖掘")]
    void TestDig()
    {
        Debug.Log($"🧪 测试挖掘：{gameObject.name} (解锁:{isUnlocked}, 类型:{soilType})");
        OnDig();
    }
}