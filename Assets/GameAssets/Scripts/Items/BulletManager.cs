using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletManager : MonoBehaviour
{
    public static BulletManager active;

    [SerializeField] 
    private GameObject burstParticle;
    [SerializeField] 
    private GameObject bulletParticle;
    [SerializeField] 
    private GameObject impactParticle;
    [SerializeField] 
    private GameObject shortImpactParticle;
    public int _count;
    public float _spread;
    private void Start()
    {
        active = this;
    }
    
    public void ShootBullets(Vector3 start, Vector3 dir, int count,float bulletDamage, float spread)
    {
        
        SpawnBurstEffect(start,dir);
        for (int i = 0; i < count; i++)
        {
            Vector3 spreadDir = dir + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));
            if (Physics.Raycast(start,spreadDir, out RaycastHit hit, 100f))
            {
                SpawnBulletEffect(start,hit.point,hit.distance);
                

                Health healthScript = hit.transform.GetComponent<Health>();
                if (healthScript != null)
                {
                    HitSound.active.HandleHitSound(healthScript, healthScript.TakeDamage(bulletDamage), hit.point);
                    SpawnImpactEffect(hit.point,hit.normal,true);
                }
                else
                {
                    SpawnImpactEffect(hit.point,hit.normal,false);
                }
            }
            else
            {
                SpawnBulletEffect(start,spreadDir.normalized * 100f,100f); 
            }
        }
        
    }

    public RaycastHit ShootInvisibleBullet(Vector3 start, Vector3 dir, float damage, float range)
    {
        if (Physics.Raycast(start,dir, out RaycastHit hit, range))
        {
            Health healthScript = hit.transform.GetComponent<Health>();
            if (healthScript != null)
            {
                HitSound.active.HandleHitSound(healthScript, healthScript.TakeDamage(damage), hit.point);
                SpawnImpactEffect(hit.point,hit.normal,true);
            }
            else
            {
                SpawnImpactEffect(hit.point,hit.normal,false);
            }

            return hit;
        }

        return new RaycastHit();
    }
    private void SpawnBurstEffect(Vector3 position, Vector3 dir)
    {
        GameObject burst = Instantiate(burstParticle);
        burst.transform.position = position;
        burst.transform.LookAt(position + dir);

        Destroy(burst,3f);
    }
    private void SpawnBulletEffect(Vector3 start, Vector3 end, float distance)
    {
        GameObject bullet = Instantiate(bulletParticle);
        bullet.transform.position = start;
        bullet.transform.LookAt(end);

        ParticleSystem.ShapeModule shape = bullet.GetComponent<ParticleSystem>().shape;
        shape.position = new Vector3(0f, 0f, distance*0.5f);
        shape.scale = new Vector3(distance*0.5f, 1f, 1f);
        
        Destroy(bullet,3f);
    }
    private void SpawnImpactEffect(Vector3 position, Vector3 normal, bool isShort)
    {
        GameObject impact = Instantiate(isShort? shortImpactParticle : impactParticle);
        impact.transform.position = position;
        impact.transform.LookAt(position + normal);

        Destroy(impact,3f);
    }
}
