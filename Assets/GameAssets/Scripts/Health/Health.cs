using UnityEngine;

public enum HealthType
{
    None,
    Player,
    Train,
    Enemy
}

public class Health : MonoBehaviour
{
    [Header("Health")]
    public HealthType healthType;
    public float health = 100;
    public float maxHealth = 100;
    public string audioOnDamageTaken;

    private Vector3 forceSum;
    private void Start()
    {
        AddHealthToGlobalList();
    }
    public void AddHealthToGlobalList()
    {
        if(EnemyManager.active != null)
            EnemyManager.active.healthTypeArray[(int)healthType].Add(this);
    }
    private void Update()
    {
        UpdateCall();
    }
    public void UpdateCall()
    {
        forceSum = Vector3.zero;
    }
    public float TakeDamage(float damage, Vector3 force = default)
    {
        if (health == 0 && damage > 0)
            return 0;

        if(audioOnDamageTaken != "")
            SoundManager.active.PlayAtPos(transform.position,audioOnDamageTaken);
        
        health -= damage;
        forceSum += force;
        if (health <= 0)
        {
            HealthReachedZero(forceSum);
            damage += health;
            health = 0;
            return damage;
        }
        else if (health > maxHealth)
        {
            damage = damage + health - maxHealth;
            health = maxHealth;
            return damage;
        }
        else
        {
            return damage;
        }
    }
    public virtual void HealthReachedZero(Vector3 force = default)   
    {
        
    }
    private void OnDestroy()
    {
        EnemyManager.active.healthTypeArray[(int)healthType].Remove(this);
    }
    public GameObject SpawnRagdoll(GameObject targetObject, GameObject ragdollPrefab, Vector3 startVelocity = default, float destroyTime = -1f)
    {
        GameObject ragdoll = Instantiate(ragdollPrefab, targetObject.transform.position, Quaternion.Euler(0, targetObject.transform.rotation.y, 0));
        if(destroyTime >= 0)
        {
            Destroy(ragdoll, destroyTime);
        }

        Transform[] targetTransformArray = targetObject.GetComponentsInChildren<Transform>();

        foreach (Rigidbody ragdolRigibody in ragdoll.GetComponentsInChildren<Rigidbody>())
        {
            int i = 0;
            for (i = 0; i < targetTransformArray.Length; i++)
            {
                if (targetTransformArray[i].name == ragdolRigibody.name)
                {
                    ragdolRigibody.transform.position = targetTransformArray[i].position;
                    ragdolRigibody.transform.rotation = targetTransformArray[i].rotation;
                    break;
                }
            }
            ragdolRigibody.linearVelocity = startVelocity;

            if (i >= targetTransformArray.Length)
            {
                Debug.LogWarning("Couldnt find limb: " + ragdolRigibody.gameObject.name);
            }
        }

        return ragdoll;
    }
}
