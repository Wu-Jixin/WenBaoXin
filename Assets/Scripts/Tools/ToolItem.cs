using UnityEngine;

public class ToolItem : MonoBehaviour
{
    [Header("工具类型")]
    public ToolType toolType = ToolType.None;

    [Header("工具名称")]
    public string toolName = "Tool";

    public ToolType GetToolType()
    {
        return toolType;
    }
}