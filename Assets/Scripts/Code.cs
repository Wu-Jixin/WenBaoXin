using UnityEngine;

public class KeepVisible : MonoBehaviour
{
    void Start()
    {
        // Ç¿ÖÆÆôÓÃäÖÈ¾Æ÷
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }
}