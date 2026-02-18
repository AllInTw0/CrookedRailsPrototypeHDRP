using UnityEngine;

public class ShopItemSO : ScriptableObject
{
    [Header("Shop Item")]
    public string nameOverride;
    public GameObject prefab;
    public string description;

    [Header("Icon Rendering")]
    public Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 origin;
    public Vector3 rotation;
    public Texture2D icon;

    public LayerMask renderLayer;
    public string GetName()
    {
        return nameOverride == "" ? this.name : nameOverride;
    }
}
