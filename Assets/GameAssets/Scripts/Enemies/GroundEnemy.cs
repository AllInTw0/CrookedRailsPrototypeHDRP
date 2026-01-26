using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : Enemy
{

    //Behaviour
    public enum State
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
                //Pack handling
                if(IsInPack() == false)
                {
                    foreach (EnemyPack pack in EnemyManager.active.activePackList)
                    {
                        float distance = Vector3.Distance(transformPos, pack.centerPos);
                        if(distance <= pack.affectRadius)
                        {
                            SetTarget(pack, GetRandomOffset(pack.sizeRadius), 1f);
                        }
                    }
                }

                //Check for nearby targets
                if (CheckForTargets(enemyInfo.sightDistance, out Health closestHealth))
                {
                    if (IsInPack()) targetPack.AngerPack(closestHealth.transform);
                    else Anger(closestHealth.transform,5f);

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
                        if (IsInPack()) SetTarget(targetPack, GetRandomOffset(targetPack.sizeRadius), 1f);
                        else SetTarget(GetRandomPosition(4f), 1f);
                        timmer = Random.Range(1.5f, 6f);
                    }
                }

                break;
            case State.Attacking:
                //Attack player / train
                if (CheckForTargets(enemyInfo.sightDistance, out Health closestHealth1))
                {
                    SetTarget(closestHealth1.transform, 1f);
                    timmer = 5f;
                }
                else
                {
                    timmer -= EnemyManager.active.behaviourRefreshRate;
                    if(timmer <= 0)
                    {
                        state = State.Idle;

                    }
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
            Vector3 samplePos = transformPos + GetRandomOffset(radius);

            if (NavMesh.SamplePosition(samplePos,out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            safety--;
        }

        Debug.LogWarning("Exeeded safety while loop limit");
        return transformPos + GetRandomOffset(radius);
    }
    public Vector3 GetRandomOffset(float radius = 4f)
    {
        Vector2 randomDir = Random.insideUnitCircle * radius;
        return new Vector3(randomDir.x, 0, randomDir.y);
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
    public void Anger(Transform target, float intrestTime)
    {
        state = State.Attacking;
        SetTarget(target, 1f);
        timmer = intrestTime;
    }
    public void SetState(State state)
    {
        this.state = state;
    }
    public State GetState()
    {
        return state;
    }
}
