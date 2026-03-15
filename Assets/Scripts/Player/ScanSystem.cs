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

        Scannable sc = target.GetComponent<Scannable>();

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

        scanText.text =
        "扫描完成\n" +
        "文物：" + currentScannable.artifactName + "\n" +
        "建议：" + currentScannable.guidanceText;

        Invoke("HidePanel", 3f);
    }

    void HidePanel()
    {
        scanPanel.SetActive(false);
    }
}