using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObjects/ItemSO", order = 1)]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    [Header("Inventory")]
    public int slotCount = 1;
    public int maxCount = 1;
    
    [Header("Tool Only")]
    public bool isTool;

    [Header("Description")]
    public string description;

    [Header("Icon Rendering")] 
    public Vector3 offset = new Vector3(0.5f,0.5f,0.5f);
    public Vector3 origin;
    public Vector3 rotation;
    public Texture2D icon;

    public LayerMask renderLayer;
}
