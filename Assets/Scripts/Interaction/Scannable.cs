using UnityEngine;

public class Scannable : MonoBehaviour
{
    [Header("文物名称")]
    public string artifactName = "未命名文物";

    [Header("扫描提示")]
    [TextArea]
    public string guidanceText = "按 F 扫描文物";

    private bool scanned = false;

    // 扫描方法
    public void Scan()
    {
        if (scanned) return;

        scanned = true;
        Debug.Log("已扫描文物: " + artifactName);
    }

    // 查询是否扫描过
    public bool IsScanned()
    {
        return scanned;
    }
}