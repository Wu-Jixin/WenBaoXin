using UnityEngine;
using System.Collections.Generic;

public class MarkingSystem : MonoBehaviour
{
    public LineRenderer linePrefab;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    public bool isActive = false; // 是否启用（由ToolSystem控制）

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartLine();
        }
        else if (Input.GetMouseButton(0))
        {
            Draw();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndLine();
        }
    }

    void StartLine()
    {
        currentLine = Instantiate(linePrefab);
        points.Clear();
    }

    void Draw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("TopSoil"))
            {
                Vector3 point = hit.point;

                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], point) > 0.05f)
                {
                    points.Add(point);
                    currentLine.positionCount = points.Count;
                    currentLine.SetPositions(points.ToArray());
                }
            }
        }
    }

    void EndLine()
    {
        currentLine = null;
    }
}