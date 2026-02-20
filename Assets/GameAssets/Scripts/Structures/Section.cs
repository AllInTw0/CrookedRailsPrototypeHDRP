using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    [SerializeField]
    private List<Connection> connectionList;
    [SerializeField]
    private float length;

    //Run-time
    [HideInInspector]
    public List<Connection> connectingConnectionList;
    [HideInInspector]
    public List<Connection> nonConnectingConnectionList;
    private void Awake()
    {
        //Initialize()
    }
    public void Initialize()
    {
        //Sort connections
        connectingConnectionList = new List<Connection>();
        nonConnectingConnectionList = new List<Connection>();

        foreach (Connection connection in connectionList)
        {
            if (connection.validConnectionNameList.Count > 0)
            {
                connectingConnectionList.Add(connection);
            }
            else
            {
                nonConnectingConnectionList.Add(connection);
            }
        }
    }
    public List<Connection> GetConnectingConnectionList()
    {
        return connectingConnectionList;
    }
    public List<Connection> GetNonConnectingConnectionList()
    {
        return nonConnectingConnectionList;
    }
    public List<Connection> GetAllConnectionList()
    {
        return connectionList;
    }
    public float GetLength()
    {
        return length;
    }
}
