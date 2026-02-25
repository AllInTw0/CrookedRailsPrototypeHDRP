using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager active;

    [SerializeField]
    private LayerMask _itemDropLayerMask;
    public static LayerMask itemDropLayerMask;
    private void Start()
    {
        itemDropLayerMask = _itemDropLayerMask;
        active = this;
    }
}
