using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetAcquiring : MonoBehaviour
{
    // How oftet acquiring happens in seconds
    [SerializeField]
    private List<Dude> agents;
    [SerializeField]
    private float frequency = 0.2f;
    private float timePassed = 0;


    public void AddUnit(Dude dude) {
        agents.Add(dude);
    }

    void Run() {
        for (int i = 0; i < agents.Count; i++)
        {
            Vector2 position = agents[i].transform.position;
            Dude agent = agents[i];
            int team = agent.Team;
            for (int j = 0; j < agents.Count; j++)
            {
                if (i != j && team != agents[j].Team) {
                    float distance = (position - (Vector2)agents[j].transform.position).magnitude;
                    print("dis = " + distance);
                    if (agent.AttackRange > distance)
                        agent.SetAttackTarget(agents[j]);
                }
            }
        }
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
