using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineGenerator : StructureGenerator
{
    [Header("Spline Generator")]
    [SerializeField]
    private Section linkedSection;
    [SerializeField]
    List<SplineVisualizer> splineVisualizerList;
    [Header("Path Shape Randomness")]
    [SerializeField]
    private Vector2 minMaxHandleLenghtMult;
    [SerializeField]
    private AnimationCurve offsetMultAlongPathLength;
    [SerializeField]
    private AnimationCurve limitOffsetAlongPathLength;
    [SerializeField]
    private float noiseFrequency;
    [SerializeField]
    private float noiseStepBasedOnDistance;
    [SerializeField]
    private Vector2 minMaxTargetOffset;
    [Header("Path End Randomness")]
    [SerializeField]
    private Vector2 minMaxDistanceForward;
    [SerializeField]
    private Vector2 minMaxDistanceRight;
    [SerializeField]
    private Vector2 minMaxRotOffset;

    public override IEnumerator Generate(StructureMaster structureMaster)
    {
        Connection startConnection = null;
        foreach (Connection connection in linkedSection.GetConnectingConnectionList())
        {
            if(connection.connectedConnection != null)
            {
                startConnection = connection;
                break;
            }
        }

        foreach (Connection endConnection in linkedSection.GetAllConnectionList())
        {
            if (endConnection == startConnection) continue;

            List<Connection> validConnections = structureMaster.GetValidConnectionsForConnection(endConnection);
            validConnections.Remove(startConnection.connectedConnection);

            if (validConnections.Count > 0)
            {
                //Connect
                Connection targetEndConnection = validConnections[Random.Range(0, validConnections.Count)];
                endConnection.connectionTransform.position = targetEndConnection.connectionTransform.position;
                endConnection.connectionTransform.rotation = Quaternion.Euler(0f, targetEndConnection.connectionTransform.eulerAngles.y - 180f, 0f);
            }
            else
            {
                //Random
                
                Vector3 endPos = startConnection.connectionTransform.position + - startConnection.connectionTransform.forward * Random.Range(minMaxDistanceForward.x, minMaxDistanceForward.y) + startConnection.connectionTransform.right * Random.Range(minMaxDistanceRight.x, minMaxDistanceRight.y);
                Debug.DrawLine(startConnection.connectionTransform.position, endPos, Color.green, 30f);
                endConnection.connectionTransform.position = endPos;
                endConnection.connectionTransform.rotation = Quaternion.Euler(0f, startConnection.connectionTransform.eulerAngles.y + Random.Range(minMaxRotOffset.x, minMaxRotOffset.y) + 180f, 0f);
            }

            CreatePath(startConnection, endConnection);
        }

        yield break;
    }
    public void CreatePath(Connection start, Connection end)
    {
        Transform startTransform = start.connectionTransform;
        Transform endTransform = end.connectionTransform;

        float dist = Vector3.Distance(startTransform.position, endTransform.position);

        Point startPoint = Spline.CreatePoint(startTransform.position, startTransform.position - startTransform.forward * (dist * Random.Range(minMaxHandleLenghtMult.x,minMaxHandleLenghtMult.y)));
        Point endPoint = Spline.CreatePoint(endTransform.position, endTransform.position + endTransform.forward * (dist * Random.Range(minMaxHandleLenghtMult.x, minMaxHandleLenghtMult.y)));

        //Calculate path
        TrackManager.CalculatePath(startPoint, endPoint, Spline.CalculateSplineLenght(startPoint, endPoint), out List<PathPoint> splinePath, 4f);
        TrackManager.CalculatePath(splinePath, out List<PathPoint> path, 4f);

        //Modify path
        float randomness = Random.value;
        float[] offsetValues = new float[path.Count];
        float maxValue = 0f;
        float pathDist = path[0].distance;
        for (int i = 1; i < path.Count - 1; i++)
        {
            pathDist += path[i].distance;
            float time = pathDist * noiseStepBasedOnDistance;
            float noise = (Mathf.PerlinNoise1D(((time + randomness) * 0.5f) * noiseFrequency) * 2f - 1f) * offsetMultAlongPathLength.Evaluate(time);
            //Debug.Log(i + " : " + noise + " : " + time);
            if (Mathf.Abs(noise) > maxValue)
                maxValue = Mathf.Abs(noise);
            offsetValues[i] = noise;
        }
        float offsetMult = Random.Range(minMaxTargetOffset.x, minMaxTargetOffset.y) / maxValue;
        //Debug.Log("maxValue: " + maxValue + ", offsetMult: " + offsetMult);
        for (int i = 1; i < path.Count-1; i++)
        {
            //Vector3 dir = (path[i].position - path[i - 1].position + path[i + 1].position - path[i].position) * 0.5f;//Average
            Vector3 dir = path[i + 1].position - path[i].position;
            Vector3 perpendicularDir = new Vector3(-dir.z, dir.y, dir.x);

            path[i].position += perpendicularDir.normalized * (offsetValues[i] < 0f ? Mathf.Max(-limitOffsetAlongPathLength.Evaluate(i / (float)path.Count), offsetValues[i] * offsetMult) : Mathf.Min(limitOffsetAlongPathLength.Evaluate(i / (float)path.Count), offsetValues[i] * offsetMult));
        }


        for (int i = 0; i < path.Count-1; i++)
        {
            Debug.DrawLine(path[i].position, path[i + 1].position, Color.purple, 30f);
        }
        for (int i = 0; i < splinePath.Count - 1; i++)
        {
            Debug.DrawLine(splinePath[i].position, splinePath[i + 1].position, Color.red, 30f);
        }

        TrackManager.CalculatePath(path, out List<PathPoint> meshPath, 1.5f);

        foreach (SplineVisualizer splineVisualizer in splineVisualizerList)
        {
            splineVisualizer.Visualize(meshPath);
        }

        //for (float i = 0; i < 1f; i+= 0.05f)
        //{
        //    Debug.DrawLine(Spline.CalculateSplinePosition(startPoint, endPoint, i), Spline.CalculateSplinePosition(startPoint, endPoint, i+ 0.05f), Color.purple,30f);
        //}
    }
}
