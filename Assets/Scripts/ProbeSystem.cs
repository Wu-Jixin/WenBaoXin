using UnityEngine;

public class ProbeSystem : MonoBehaviour
{
    public bool isActive = false;
    int probeCount = 0;

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

                // ⭐ 标记为已探测
                TopSoilController soil = hit.collider.GetComponent<TopSoilController>();
                if (soil != null)
                {
                    soil.SetProbed();
                }
            }
        }
    }

    void CreateHole(RaycastHit hit)
    {
        probeCount++;

        Vector3 pos = hit.point;
        pos.y -= 0.05f;

        GameObject hole = Instantiate(holePrefab, pos, Quaternion.identity);

        // 设置名字（调试用）
        hole.name = "ProbeHole_" + probeCount;

        // ⭐ 在孔上显示编号（3D文字）
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(hole.transform);
        textObj.transform.localPosition = new Vector3(0, 0.15f, 0);

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = probeCount.ToString();
        text.characterSize = 0.1f;
        text.fontSize = 50;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
    }
    void DetectResult(RaycastHit hit)
    {
        float radius = 0.5f; // 探测范围

        Collider[] hits = Physics.OverlapSphere(hit.point, radius);

        bool foundArtifact = false;

        foreach (var col in hits)
        {
            if (col.CompareTag("Artifact"))
            {
                foundArtifact = true;
                break;
            }
        }

        string resultText = foundArtifact ?
            "探测结果：疑似文物" :
            "探测结果：正常土层";

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGuidance(resultText);
        }
    }

    void LateUpdate()
    {
        foreach (TextMesh t in FindObjectsOfType<TextMesh>())
        {
            t.transform.rotation = Camera.main.transform.rotation;
        }
    }
}