using System.Collections;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    public virtual IEnumerator Generate(StructureMaster structureMaster)
    {
        return null;
    }
    public void GenerateTest()
    {
        StartCoroutine(Generate(null));
    }
}
