using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dude : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Transform destination;

    public void Init(Transform destination) {
        navMeshAgent = GetComponent<NavMeshAgent>();
        this.destination = destination;
        navMeshAgent.destination = destination.position;
    }

    // void Update() {
    //     navMeshAgent.destination = destination.position;
    // }
}
