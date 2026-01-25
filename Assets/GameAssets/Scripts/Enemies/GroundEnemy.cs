using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : Enemy
{

    //Behaviour
    enum State
    {
        Idle,
        Attacking,
    }

    private State state = State.Idle;
    private float timmer = 0; //Diffrent usses through out the states
    public override void FixedUpdateCall()
    {
        base.FixedUpdateCall();
    }
    public override void UpdateCall()
    {
        base.UpdateCall();
    }
    public override void UpdateBehaviour()
    {
        bool targetReached = CheckDistanceBehavour();
        //Debug.Log(targetReached);
        Debug.DrawLine(transformPos, GetTargetPosition(), targetReached ? Color.green : Color.red, EnemyManager.active.behaviourRefreshRate);

        switch (state)
        {
            case State.Idle:
                //Check for nearby targets
                if (CheckForTargets(enemyInfo.sightDistance, out Health closestHealth))
                {
                    state = State.Attacking;
                    SetTarget(closestHealth.transform, 1f);
                    timmer = 0f;
                }
                else
                {
                    //Roam around
                    if (targetReached)
                    {
                        timmer -= EnemyManager.active.behaviourRefreshRate;
                    }
                    if (timmer <= 0)
                    {
                        SetTarget(GetRandomPosition(4f), 1f);
                        timmer = Random.Range(3f, 6f);
                    }
                }
                

                break;
            case State.Attacking:
                //Attack player / train
                if (CheckForTargets(enemyInfo.sightDistance, out Health closestHealth1))
                {
                    SetTarget(closestHealth1.transform, 1f);
                }
                break;
            default:
                break;
        }

        UpdateNavigationBehavour();
    }
    public Vector3 GetRandomPosition(float radius = 4f)
    {
        int safety = 5;
        while (safety > 0)
        {
            Vector2 randomDir = Random.insideUnitCircle * radius;
            Vector3 samplePos = transformPos + new Vector3(randomDir.x, 0, randomDir.y);

            if (NavMesh.SamplePosition(samplePos,out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            safety--;
        }

        Debug.LogWarning("Exeeded safety while loop limit");
        Vector2 randomDir1 = Random.insideUnitCircle * radius;
        return transformPos + new Vector3(randomDir1.x, 0, randomDir1.y);
    }
    public bool CheckForTargets(float maxDistance, out Health closestHealth)
    {
        closestHealth = null;
        float closestDistance = float.PositiveInfinity;
        foreach (EnemySO.HealthWeightParams healthWeight in enemyInfo.HealthWeightParamsList)
        {
            foreach (Health health in EnemyManager.active.healthTypeArray[(int)healthWeight.healthType])
            {
                if (health == this.health)
                    continue;

                float distance = Vector3.Distance(transformPos, health.transform.position);

                if (distance < maxDistance && (distance / healthWeight.importanceWeight) < closestDistance)
                {
                    closestHealth = health;
                    closestDistance = distance / healthWeight.importanceWeight;
                }
            }
        }

        if (closestHealth)
            return true;
        return false;
    }
}
