using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestDude : MonoBehaviour
{
    
    [SerializeField]    
    private NavMeshAgent navMeshAgent;
    [SerializeField]    
    private Transform destination;

    void Start()
    {
        navMeshAgent.updateRotation = false;
    }

    void Update()
    {
        transform.LookAt(destination.position);
        navMeshAgent.SetDestination(destination.position);
    }
}
