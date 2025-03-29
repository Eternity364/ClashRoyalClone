using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TempNavMeshTest : MonoBehaviour
{
    [SerializeField]    
    private NavMeshAgent navMeshAgent;
    [SerializeField]    
    private Transform destination;


    void Start()
    {
        navMeshAgent.destination = destination.position;
        navMeshAgent.updateRotation = false;
    }
}
