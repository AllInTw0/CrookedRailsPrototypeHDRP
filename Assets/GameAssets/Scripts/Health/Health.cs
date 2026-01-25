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
    public HealthType healthType;
    public float health = 100;
    public float maxHealth = 100;
    public string audioOnDamageTaken;

    private void Start()
    {
        AddHealthToGlobalList();
    }
    public void AddHealthToGlobalList()
    {
        EnemyManager.active.healthTypeArray[(int)healthType].Add(this);
    }
    public float TakeDamage(float damage)
    {
        if (health == 0)
            return 0;
        
        if(audioOnDamageTaken != "")
            SoundManager.active.PlayAtPos(transform.position,audioOnDamageTaken);
        
        health -= damage;
        if (health <= 0)
        {
            HealthReachedZero();
            damage += health;
            health = 0;
            return damage;
        }
        else
        {
            return damage;
        }
    }
    public virtual void HealthReachedZero()   
    {
        
    }
    private void OnDestroy()
    {
        EnemyManager.active.healthTypeArray[(int)healthType].Remove(this);
    }
}
