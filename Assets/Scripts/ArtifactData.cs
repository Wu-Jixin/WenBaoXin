using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactData", menuName = "WenBao/Artifact")]
public class ArtifactData : ScriptableObject
{
    public string artifactName;   // 文物名称
    public string period;         // 时期
    [TextArea]
    public string description;    // 描述（可扩展）
}