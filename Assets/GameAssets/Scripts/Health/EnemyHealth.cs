using UnityEngine;

public class EnemyHealth : Health
{
    [Header("Enemy")]
    [SerializeField] 
    private EnemyBehaviour behaviour;
    [SerializeField] 
    private float destroyDelay;

    public override void HealthReachedZero()
    {
        EnemyManager.active.RemoveEnemy(behaviour);
        behaviour.Freeze();
        Destroy(gameObject,destroyDelay);
    }
}
