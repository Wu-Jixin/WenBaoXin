using UnityEngine;

public class ProbeSystem : MonoBehaviour
{
    public bool isActive = false;

    [Header("探孔设置")]
    public GameObject holePrefab;
    public float maxDistance = 5f;

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Probe();
        }
    }

    void Probe()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.CompareTag("TopSoil"))
            {
                CreateHole(hit);
                DetectResult(hit);
            }
        }
    }

    void CreateHole(RaycastHit hit)
    {
        Vector3 pos = hit.point;

        // 稍微往下压一点，避免悬浮
        pos.y -= 0.05f;

        Instantiate(holePrefab, pos, Quaternion.identity);
    }

    void DetectResult(RaycastHit hit)
    {
        // ⭐ 模拟探测结果（后面可以改成真实判断）
        float random = Random.value;

        if (random < 0.3f)
        {
            Debug.Log("🟡 探测结果：疑似文物");
        }
        else
        {
            Debug.Log("⚪ 探测结果：正常土层");
        }
    }
}