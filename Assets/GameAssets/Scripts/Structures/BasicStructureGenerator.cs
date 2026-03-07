using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicStructureGenerator : StructureGenerator
{
    public List<GenerationEntry> generationEntryList;

    public override IEnumerator Generate(StructureMaster structureMaster)
    {
        foreach (GenerationEntry generationEntry in generationEntryList)
        {
            if (generationEntry.countType == GenerationEntry.CountType.minMaxRandom)
            {
                int count = Random.Range(generationEntry.minMaxCount.x, generationEntry.minMaxCount.y + 1); //+1 because maxExclusive

                for (int i = 0; i < count; i++)
                {
                    //Spawn section
                    GameObject randomPrefab = generationEntry.sectionPrefabList[Random.Range(0, generationEntry.sectionPrefabList.Count)];
                    Section spawnedSection = structureMaster.SpawnSection(randomPrefab);
                    //Invoke section scripts
                    foreach (StructureGenerator structureGenerator in spawnedSection.GetComponents<StructureGenerator>())
                    {
                        yield return StartCoroutine(structureGenerator.Generate(structureMaster));
                    }
                    structureMaster.AddSectionToStructure(spawnedSection); //This is here so i can call the coroutines first then do the finishing touches
                }
            }
            else if (generationEntry.countType == GenerationEntry.CountType.fillLenght)
            {
                float length = structureMaster.GetLength(generationEntry.lengthType) + generationEntry.lengthAddition;
                int safety = 30;
                while (length > 0 && safety > 0)
                {
                    //Spawn section
                    GameObject randomPrefab = generationEntry.sectionPrefabList[Random.Range(0, generationEntry.sectionPrefabList.Count)];
                    Section spawnedSection = structureMaster.SpawnSection(randomPrefab);
                    //Invoke section scripts
                    foreach (StructureGenerator structureGenerator in spawnedSection.GetComponents<StructureGenerator>())
                    {
                        yield return StartCoroutine(structureGenerator.Generate(structureMaster));
                    }
                    structureMaster.AddSectionToStructure(spawnedSection); //This is here so i can call the coroutines first then do the finishing touches

                    length -= spawnedSection.GetLength();
                    safety--;
                }
                if (safety <= 0f) Debug.LogWarning("Safety == 0!");
            }
            yield return new WaitForSeconds(0.4f);
        }
        structureMaster.SpawnEndPrefabs();
    }
}
