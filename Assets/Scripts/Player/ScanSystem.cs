using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScanSystem : MonoBehaviour
{
    public PlayerAiming aiming;

    public GameObject scanPanel;
    public Slider scanBar;
    public TMP_Text scanText;

    public float scanTime = 2f;

    private bool isScanning = false;
    private float scanTimer = 0f;
    private Scannable currentScannable;

    void Start()
    {
        scanPanel.SetActive(false);
    }

    void Update()
    {
        if (!isScanning)
        {
            DetectScanInput();
        }
        else
        {
            ScanningProcess();
        }
    }

    void DetectScanInput()
    {
        GameObject target = aiming.GetCurrentTarget();

        if (target == null) return;

        Scannable sc = target.GetComponentInParent<Scannable>();

        if (sc != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartScan(sc);
            }
        }
    }

    void StartScan(Scannable sc)
    {
        isScanning = true;
        scanTimer = 0f;
        currentScannable = sc;

        scanPanel.SetActive(true);
        scanBar.value = 0;

        scanText.text = "扫描中...";
    }

    void ScanningProcess()
    {
        scanTimer += Time.deltaTime;

        float progress = scanTimer / scanTime;

        scanBar.value = progress;

        if (scanTimer >= scanTime)
        {
            FinishScan();
        }
    }

    void FinishScan()
    {
        isScanning = false;

        scanText.text = "扫描完成";

        Debug.Log("=== 扫描完成，开始识别文物 ===");

        if (currentScannable == null)
        {
            Debug.LogError("❌ currentScannable 是空！");
            return;
        }

        // ⭐ 获取 ArtifactTag（用更稳的方法）
        ArtifactTag tag = currentScannable.GetComponentInParent<ArtifactTag>();

        if (tag == null)
        {
            Debug.LogError("❌ 没找到 ArtifactTag！");
            return;
        }

        Debug.Log($"✅ 找到 ArtifactTag: {tag.voxelTag}");

        if (LossReportSystem.Instance == null)
        {
            Debug.LogError("❌ LossReportSystem.Instance 是空！");
            return;
        }

        Debug.Log("✅ 调用 ShowReport");

        LossReportSystem.Instance.ShowReport(tag.voxelTag);

        Invoke("HidePanel", 1.5f);
    }

    void HidePanel()
    {
        scanPanel.SetActive(false);
    }
}