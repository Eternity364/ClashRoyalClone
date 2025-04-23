using System.Collections.Generic;
using Assets.Scripts.Unit;
using UnityEngine;

public class TargetAcquiring : MonoBehaviour
{
    // How often acquiring happens in seconds
    [SerializeField]
    private List<Unit> agents;
    [SerializeField]
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
            if (agent.IsDead) continue;
            int team = agent.Team;
            for (int j = 0; j < agents.Count; j++)
            {
                if (agents[j].IsDead || (agent.HasTarget && agent.Target != enemyBases[agent.Team])) continue;
                if (i != j && team != agents[j].Team) {
                    if (CheckAndSetTarget(agent, agents[j])) break;
                }
            }
            
            if (!agent.HasTarget) {
               CheckAndSetTarget(agent, enemyBases[agent.Team]); 
            }
        }
        // Units shouldn't be removed while we are iterating through them.
        removeLock = false;
        for (int i = 0; i < toRemove.Count; i++) {
            agents.Remove(toRemove[i]);
        }
        toRemove.Clear();
    }

    private bool CheckAndSetTarget(Unit attacker, Unit possibleEnemy) {
        Vector3 position = attacker.transform.position;
        float distance = (position - possibleEnemy.transform.position).magnitude;
        bool targetValid = false;
        if ((!attacker.HasTarget || attacker.Target == enemyBases[attacker.Team]) && attacker.AttackRange > distance) {
            attacker.SetAttackTarget(possibleEnemy);
            targetValid = true;
        }
        return targetValid;
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
