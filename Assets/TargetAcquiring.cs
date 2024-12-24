using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TargetAcquiring : MonoBehaviour
{
    // How oftet acquiring happens in seconds
    [SerializeField]
    private List<Dude> agents;
    [SerializeField]
    private float frequency = 0.2f;
    private float timePassed = 0;
    private List<Dude> toRemove = new();
    bool removeLock;

    void Start() {
        foreach (Dude dude in agents)
        {
            dude.OnDeath += RemoveUnit;
        }
    }

    public void AddUnit(Dude dude) {
        agents.Add(dude);
        dude.OnDeath += RemoveUnit;
    }

    public void RemoveUnit(Dude dude) {
        if (removeLock)
            toRemove.Add(dude);
        else {
            agents.Remove(dude);
        }
    }

    void Run() {
        removeLock = true;
        
        print("agents count = " + agents.Count);
        for (int i = 0; i < agents.Count; i++)
        {
            Vector2 position = agents[i].transform.position;
            Dude agent = agents[i];
            if (agent.IsDestroyed()) continue;
            int team = agent.Team;
            for (int j = 0; j < agents.Count; j++)
            {
                if (agents[j].IsDestroyed()) continue;
                if (i != j && team != agents[j].Team) {
                    float distance = (position - (Vector2)agents[j].transform.position).magnitude;
                    if (!agent.HasTarget && agent.AttackRange > distance)
                        agent.SetAttackTarget(agents[j]);
                }
            }
        }
        removeLock = false;
        for (int i = 0; i < toRemove.Count; i++) {
            agents.Remove(toRemove[i]);
        }
        toRemove.Clear();
    }

    // Update is called once per frame
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
