using UnityEngine;

public class Cleanable : MonoBehaviour
{
    [Header("灰尘层对象")]
    public GameObject dustLayer;

    [Header("清理速度")]
    public float cleanSpeed = 0.3f;

    [Header("当前清理进度")]
    [Range(0f, 1f)]
    public float cleanProgress = 0f;

    private bool isCleaned = false;

    private Renderer dustRenderer;
    private Color originalColor;

    private ToolSystem toolSystem;

    void Start()
    {
        // 查找 ToolSystem
        toolSystem = FindObjectOfType<ToolSystem>();

        if (toolSystem == null)
        {
            Debug.LogWarning("未找到 ToolSystem，清理功能可能无法正确工作");
        }

        // 获取灰尘渲染器
        if (dustLayer != null)
        {
            dustRenderer = dustLayer.GetComponent<Renderer>();

            if (dustRenderer != null)
            {
                originalColor = dustRenderer.material.color;
            }
        }
        else
        {
            Debug.LogWarning("Cleanable: 未设置 DustLayer");
        }
    }

    void Update()
    {
        if (isCleaned) return;

        // 只有毛刷才能清理
        if (toolSystem != null && toolSystem.IsCurrentTool(ToolSystem.ToolType.Brush))
        {
            if (Input.GetMouseButton(0))
            {
                Clean();
            }
        }
    }

    void Clean()
    {
        cleanProgress += Time.deltaTime * cleanSpeed;

        cleanProgress = Mathf.Clamp01(cleanProgress);

        UpdateDustVisual();

        if (cleanProgress >= 1f)
        {
            FinishCleaning();
        }
    }

    void UpdateDustVisual()
    {
        if (dustRenderer == null) return;

        float alpha = 1f - cleanProgress;

        Color c = originalColor;
        c.a = alpha;

        dustRenderer.material.color = c;
    }

    void FinishCleaning()
    {
        isCleaned = true;

        if (dustLayer != null)
        {
            dustLayer.SetActive(false);
        }

        Debug.Log("文物清理完成：" + gameObject.name);
    }

    // 供其他系统读取进度
    public float GetCleanProgress()
    {
        return cleanProgress;
    }

    public bool IsCleaned()
    {
        return isCleaned;
    }
}