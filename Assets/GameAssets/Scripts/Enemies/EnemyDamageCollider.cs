using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDamageCollider : MonoBehaviour
{
    [SerializeField] 
    private EnemyBehaviour enemyBehaviour;
    [SerializeField] 
    private float coolDown = 0.2f;
    [SerializeField] 
    private float freezeTime = 0.1f;
    [SerializeField] 
    private float attackDelay = 0.1f;
    [SerializeField] 
    private float attackDamage = 5f;
    [SerializeField] 
    private string attackSoundName;
    [SerializeField] 
    private string attackImpactSoundName;
    
    //Run Time
    private float coolDownTime;

    private void Update()
    {
        coolDownTime -= Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (coolDownTime < 0f)
        {
            Health healthScript = other.GetComponent<Health>();
            if (healthScript != null)
            {
                SoundManager.active.PlayAtPos(transform.position,attackSoundName);
                
                coolDownTime = coolDown;
                enemyBehaviour.Freeze();
                
                Invoke(nameof(UnFreeze),freezeTime);
                
                StartCoroutine(DamageDelayed(healthScript,attackDamage,attackDelay));
            }
        }
    }

    private void UnFreeze()
    {
        enemyBehaviour.UnFreeze();
    }

    IEnumerator DamageDelayed(Health healthScript, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        healthScript.TakeDamage(damage);
        SoundManager.active.PlayAtPos(transform.position,attackImpactSoundName);
    }
}
