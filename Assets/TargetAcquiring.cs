using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private float timePassed = 0;
    private List<Unit> toRemove = new();
    bool removeLock;

    void Start() {
        foreach (Unit unit in agents)
        {
            unit.OnDeath += RemoveUnit;
        }
    }

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
            if (agent.IsDestroyed()) continue;
            int team = agent.Team;
            for (int j = 0; j < agents.Count; j++)
            {
                if (agents[j].IsDestroyed() || (agent.HasTarget && agent.Target != enemyBases[agent.Team])) continue;
                if (i != j && team != agents[j].Team) {
                    CheckPosition(agent, agents[j]);
                }
            }
            
            if (!agent.HasTarget) {
               CheckPosition(agent, enemyBases[agent.Team]); 
            }
        }
        removeLock = false;
        for (int i = 0; i < toRemove.Count; i++) {
            agents.Remove(toRemove[i]);
        }
        toRemove.Clear();
    }

    private void CheckPosition(Unit attacker, Unit possibleEnemy) {
        Vector2 position = attacker.transform.position;
        float distance = (position - (Vector2)possibleEnemy.transform.position).magnitude;
        if ((!attacker.HasTarget || attacker.Target == enemyBases[attacker.Team]) && attacker.AttackRange > distance)
            attacker.SetAttackTarget(possibleEnemy);
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
