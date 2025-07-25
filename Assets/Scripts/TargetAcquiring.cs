using System.Collections.Generic;
using Units;
using UnityEngine;

public class TargetAcquiring : MonoBehaviour
{
    // How often acquiring happens in seconds
    [SerializeField]
    private List<Unit> agents;
    [SerializeField]
    private float frequency = 0.2f;
    [SerializeField]
    [Tooltip("If a new possible target is closer than this value, it will be set as a target.")]
    private float distanceDifferanceToChangeTarget = 2f;
    [SerializeField]
    private ObjectPool objectPool;

    private float timePassed = 0;
    private List<Unit> toRemove = new();
    bool removeLock;

    public void AddUnit(Unit unit) {
        agents.Add(unit);
        unit.OnDeath += RemoveUnit;
    }

    public void RemoveUnit(Unit unit) {
        if (removeLock)
            toRemove.Add(unit);
        else {
            agents.Remove(unit);
        }
    }

    private void Run() {
        removeLock = true;
        for (int i = 0; i < agents.Count; i++)
        {
            Unit agent = agents[i];

            if (agent.IsDead) continue;

            Unit closestEnemy = null;
            float closestDistance = float.MaxValue;
            float currentEnemyDistance = float.MaxValue;
            if (agent.HasTarget && agent.Target != UnitSpawner.Instance.Bases[agent.Team]) {
                currentEnemyDistance = (agent.transform.position - agent.Target.transform.position).magnitude;
            }
            int team = agent.Team;

            if (agent.AllowedTargets != AllowedTargets.Base)
            {
                for (int j = 0; j < agents.Count; j++)
                {
                    if (agents[j].IsDead) continue;
                    if (i != j && team != agents[j].Team)
                    {
                        float distance = (agent.transform.position - agents[j].transform.position).magnitude;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestEnemy = agents[j];
                        }
                    }

                    if (closestEnemy != null && agent.Target != closestEnemy && closestDistance < currentEnemyDistance - distanceDifferanceToChangeTarget)
                    {
                        CheckAndSetTarget(agent, closestEnemy, closestDistance);
                    }
                }
            }
            
            if (!agent.HasTarget)
            {
                CheckAndSetTarget(agent, UnitSpawner.Instance.Bases[agent.Team], (agent.transform.position - UnitSpawner.Instance.Bases[agent.Team].transform.position).magnitude);
            }
        }
        // Units shouldn't be removed while we are iterating through them.
        removeLock = false;
        for (int i = 0; i < toRemove.Count; i++) {
            agents.Remove(toRemove[i]);
        }
        toRemove.Clear();
    }

    private void CheckAndSetTarget(Unit attacker, Unit possibleEnemy, float distance) {
        if (attacker.AttackNoticeRange > distance) {
            attacker.SetAttackTarget(possibleEnemy, possibleEnemy is not Base);
        }
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed > frequency) 
        {
            timePassed -= frequency;
            Run();
        }
    }
}
