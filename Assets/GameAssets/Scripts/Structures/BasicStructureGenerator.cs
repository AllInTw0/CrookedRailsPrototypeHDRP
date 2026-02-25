using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicStructureGenerator : StructureGenerator
{
    public List<GenerationEntry> generationEntryList;

    public override void Generate(StructureMaster structureMaster)
    {
        StartCoroutine(GenerateIEnumerator(structureMaster));
    }

    IEnumerator GenerateIEnumerator(StructureMaster structureMaster)
    {      
        foreach (GenerationEntry generationEntry in generationEntryList)
        {
            Section SpawnSection()
            {
                GameObject randomPrefab = generationEntry.sectionPrefabList[Random.Range(0, generationEntry.sectionPrefabList.Count)];
                return structureMaster.SpawnSection(randomPrefab);
            }

            if (generationEntry.countType == GenerationEntry.CountType.minMaxRandom)
            {
                int count = Random.Range(generationEntry.minMaxCount.x, generationEntry.minMaxCount.y + 1); //+1 because maxExclusive

                for (int i = 0; i < count; i++)
                {
                    SpawnSection();
                }
            }
            else if (generationEntry.countType == GenerationEntry.CountType.fillLenght)
            {
                float length = structureMaster.GetLength(generationEntry.lengthType) + generationEntry.lengthAddition;
                int safety = 30;
                while (length > 0 && safety > 0)
                {
                    Section section = SpawnSection();
                    length -= section.GetLength();
                    safety--;
                }
                if (safety <= 0f) Debug.LogWarning("Safety == 0!");
            }
            yield return new WaitForSeconds(0.4f);
        }
        structureMaster.SpawnEndPrefabs();
    }
}
