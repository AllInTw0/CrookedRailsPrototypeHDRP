using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public static class Util
{
    public static Collider[] PhysicsBoxColliderOverlap(BoxCollider boxCollider, LayerMask layerMask = default)
    {
        return Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), boxCollider.size * 0.5f, boxCollider.transform.rotation, layerMask);
    }
}
