using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100;
    public float maxHealth = 100;
    public string audioOnDamageTaken;
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
}
