using UnityEngine;

public class ShowPosition : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"TestWall 位置: {transform.position}");
        Debug.Log($"TestWall 是否激活: {gameObject.activeInHierarchy}");

        // 检查渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log($"渲染器存在，是否启用: {renderer.enabled}");
        }
        else
        {
            Debug.Log("没有渲染器！");
        }

        // 检查所有子物体
        foreach (Transform child in transform)
        {
            Debug.Log($"子物体: {child.name}, 位置: {child.position}");
        }
    }

    void Update()
    {
        // 按L键显示位置
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"当前位置: {transform.position}");
        }
    }
}