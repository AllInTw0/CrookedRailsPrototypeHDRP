using UnityEngine;

[CreateAssetMenu(fileName = "StructureSO", menuName = "ScriptableObjects/StructureSO", order = 2)]
public class StructureSO : ScriptableObject
{
    public GameObject structurePrefab;

    [Header("AutoStop")]
    public bool addAutoStop;
    public AutoStopType stopType;
    public float stopOffset;
}
