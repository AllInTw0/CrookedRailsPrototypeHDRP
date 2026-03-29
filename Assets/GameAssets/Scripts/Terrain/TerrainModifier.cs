using System.Collections;
using System.IO;
using UnityEngine;

public class TerrainModifier : StructureGenerator
{
    public BoxCollider boxCollider;
    public AnimationCurve shapeBasedOnDistance;
    public float influenceDistance;
    public override IEnumerator Generate(StructureMaster structureMaster)
    {
        Vector2Int chunkCoord = TerrainGeneration.active.GetChunkCoord(transform.position);

        int exploreDist = Mathf.RoundToInt(influenceDistance / TerrainGeneration.chunkSize + 0.5f);
        if (exploreDist < 1)
            exploreDist = 1;

        for (int chunkX = -exploreDist; chunkX < exploreDist; chunkX++)
        {
            for (int chunkY = -exploreDist; chunkY < exploreDist; chunkY++)
            {
                TerrainGeneration.Chunk chunk = TerrainGeneration.active.CreateOrGetChunk(new Vector2Int(chunkCoord.x + chunkX, chunkCoord.y + chunkY));

                for (int x = 0; x < chunk.heightMap.GetLength(0); x++)
                {
                    for (int y = 0; y < chunk.heightMap.GetLength(1); y++)
                    {
                        Vector3 vertWorldPos = chunk.GetVertexWorldPos(x, y);

                        float distance = Vector2.Distance(new Vector2(vertWorldPos.x, vertWorldPos.z), new Vector2(transform.position.x, transform.position.z));

                        if (distance < influenceDistance && (boxCollider == null || Util.PointInBox(vertWorldPos, boxCollider)))
                        {
                            float time = shapeBasedOnDistance.Evaluate(distance);
                            chunk.heightMap[x, y] = Mathf.Lerp(vertWorldPos.y, transform.position.y, time);
                        }
                        Debug.DrawRay(vertWorldPos, Vector3.up, Color.blue, 20f);
                    }
                }
            }
        }
        for (int chunkX = -exploreDist; chunkX < exploreDist; chunkX++)
        {
            for (int chunkY = -exploreDist; chunkY < exploreDist; chunkY++)
            {
                TerrainGeneration.Chunk chunk = TerrainGeneration.active.CreateOrGetChunk(new Vector2Int(chunkCoord.x + chunkX, chunkCoord.y + chunkY));
                chunk.GenerateMesh();
            }
        }
        yield break;
    }

}
