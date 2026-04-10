using UnityEngine;

public class TopSoilController : MonoBehaviour
{
    [Header("状态控制")]
    public bool isMarked = false;   // 是否已经划线
    public bool isProbed = false;   // 是否已经探测

    [Header("挖掘设置")]
    public bool allowDig = false;

    public void SetMarked()
    {
        isMarked = true;
        CheckDigCondition();
    }

    public void SetProbed()
    {
        isProbed = true;
        CheckDigCondition();
    }

    void CheckDigCondition()
    {
        // 规则：必须划线 + 探测后才能挖
        if (isMarked && isProbed)
        {
            allowDig = true;
            Debug.Log("✅ 可以开始挖掘表层土");
        }
    }
}