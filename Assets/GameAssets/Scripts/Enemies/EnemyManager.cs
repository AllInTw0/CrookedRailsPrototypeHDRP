using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager active;
    
    //Variables
    [Header("Temp")]
    [SerializeField] 
    private GameObject enemyPrefab;
    [FormerlySerializedAs("enemyCount")] [SerializeField] 
    private int enemyCount_Debug;
    [FormerlySerializedAs("targetEnemyCount")] [SerializeField] 
    private int targetEnemyCount_Debug;
    [Header("Updating")]
    [SerializeField] 
    public float behaviourRefreshRate = 0.5f;
    [Header("Spawning Area")] 
    [SerializeField] 
    private Vector2 minMaxSpawnAreaRot;
    [SerializeField] 
    private Vector2 minMaxSpawnAreaDistance;
    [Header("Spawning Interval")] 
    [SerializeField]
    private float spawnCoolDown;
    [Header("Enemy Code Refrences")]
    [SerializeField]
    public LayerMask groundLayer;
    [SerializeField]
    public float pathFindingTriggerMovedDistance = 0.35f;

    //Run Time
    private List<Enemy> enemyList = new List<Enemy>();
    public List<Health>[] healthTypeArray;

    private float spawnCoolDownTime;
    private int j;

    private void Awake()
    {
        active = this;
        healthTypeArray = new List<Health>[System.Enum.GetValues(typeof(HealthType)).Length];
        for (int i = 0; i < healthTypeArray.Length; i++)
        {
            healthTypeArray[i] = new List<Health>();
        }
        j = enemyCount_Debug;
        InvokeRepeating(nameof(UpdateEnemyBehaviour),0f,behaviourRefreshRate);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].FixedUpdateCall();
        }
    }

    void Update()
    {
        if (j > 0)
        {
            enemyList.Add(Instantiate(enemyPrefab, new Vector3(Random.Range(-3f,3f),Random.Range(0f,6f),Random.Range(-3f,3f)), Quaternion.identity).transform.GetComponent<Enemy>());
            j--;
        }

        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].UpdateCall();
        }

        if (enemyList.Count < targetEnemyCount_Debug && j <= 0)
        {
            enemyList.Add(Instantiate(enemyPrefab, new Vector3(Random.Range(-3f,3f),Random.Range(0f,6f),Random.Range(-3f,3f)), Quaternion.identity).transform.GetComponent<Enemy>());
        }
        
        //Spawning
        spawnCoolDownTime -= Time.deltaTime;
        if (spawnCoolDownTime <= 0f && GameStateManager.canEnemiesSpawn)
        {
            for (int i = 0; i < Random.Range(5,15); i++)
            {
                float distance = Random.Range(minMaxSpawnAreaDistance.x, minMaxSpawnAreaDistance.y);
                float rot = Random.Range(minMaxSpawnAreaRot.x, minMaxSpawnAreaRot.y);

                RailCar frontRailCar = GenerationManager.active.playerTrain.GetRailCarAtIndex(0);
                rot += frontRailCar.transform.eulerAngles.y;

                Vector3 dir = Quaternion.AngleAxis(rot, Vector3.up) * Vector3.forward;
                if (Physics.Raycast(frontRailCar.transform.position + Vector3.up * 50f + dir * distance, Vector3.down, out RaycastHit hit, 100f))
                {
                    //Spawn Enemy
                    Debug.DrawRay(frontRailCar.transform.position + Vector3.up * 50f + dir * distance,Vector3.down*100f,Color.blueViolet,60f);
                    SpawnEnemy(enemyPrefab,hit.point,Quaternion.identity);
                }
                else
                    i++;
            }
            
            spawnCoolDownTime = spawnCoolDown;
        }
    }

    private void UpdateEnemyBehaviour()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].UpdateBehaviour();
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemyList.Remove(enemy);
    }

    public Enemy[] GetEnemies()
    {
        return enemyList.ToArray();
    }

    public void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, rotation);
        enemyList.Add(enemy.transform.GetComponent<Enemy>());
    }
}
