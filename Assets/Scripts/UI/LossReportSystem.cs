// Assets/02_Scripts/Core/LossReportSystem.cs
using UnityEngine;
using UnityEngine.UI;

public class LossReportSystem : MonoBehaviour
{
    public static LossReportSystem Instance;

    [Header("UI组件")]
    public GameObject reportPanel;
    public Text titleText;
    public Text descriptionText;
    public Image illustrationImage;

    [Header("报告内容")]
    public ReportEntry[] reportEntries;

    // 将ReportEntry类移到外部，或者不使用Header特性
    [System.Serializable]
    public class ReportEntry
    {
        public string voxelTag;
        public string title;
        public string description;
        public Sprite illustration;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }
    }

    public void ShowReport(string voxelTag)
    {
        foreach (ReportEntry entry in reportEntries)
        {
            if (entry.voxelTag == voxelTag)
            {
                if (titleText != null)
                    titleText.text = entry.title;

                if (descriptionText != null)
                    descriptionText.text = entry.description;

                if (illustrationImage != null && entry.illustration != null)
                {
                    illustrationImage.sprite = entry.illustration;
                    illustrationImage.gameObject.SetActive(true);
                }

                if (reportPanel != null)
                {
                    reportPanel.SetActive(true);
                    Invoke("HideReport", 4f);
                }

                Debug.Log($"显示报告: {entry.title}");
                return;
            }
        }

        Debug.LogWarning($"未找到标签对应的报告: {voxelTag}");
    }

    public void HideReport()
    {
        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }
    }
}