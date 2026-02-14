using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    //Serialized
    
    //Run Time
    public static bool isStartingLocationSpawned;
    public static bool isStationSpawned;

    public static int currentLevel = 1;

    public static bool canEnemiesSpawn
    {
        get
        {
            return !(isStartingLocationSpawned || isStationSpawned);
        }
    }


    private void Start()
    {
        Money.SetStartingMoney();
    }
}
