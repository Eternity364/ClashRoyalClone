using System.Collections.Generic;
using Assets.Scripts.Unit;
using UnityEngine;

public class TargetAcquiring : MonoBehaviour
{
    // How often acquiring happens in seconds
    [SerializeField]
    private List<Unit> agents;
    [SerializeField]
    [Tooltip("Set the enemy bases in the same order as the teams. So for team 0 enemy base should be team 1 and otherwise")]
    private List<Unit> enemyBases;
    [SerializeField]
    private float frequency = 0.2f;
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
            Unit closestEnemy = null;
            float closestDistance = float.MaxValue;
            if (agent.IsDead) continue;
            int team = agent.Team;

            if (agent is Melee && agent.Team == 0) {
                int a = 9;
            }

            for (int j = 0; j < agents.Count; j++)
            {
                if (agents[j].IsDead) continue;
                if (i != j && team != agents[j].Team) {
                   float distance = (agent.transform.position - agents[j].transform.position).magnitude;
                   if (distance < closestDistance) {
                       closestDistance = distance;
                       closestEnemy = agents[j];
                   }
                }

                if (closestEnemy != null && agent.Target != closestEnemy) {
                    CheckAndSetTarget(agent, closestEnemy, closestDistance);
                }
            }
            
            if (!agent.HasTarget) {
               CheckAndSetTarget(agent, enemyBases[agent.Team], (agent.transform.position - enemyBases[agent.Team].transform.position).magnitude); 
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
            attacker.SetAttackTarget(possibleEnemy);
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
