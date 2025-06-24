using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class TestDude : MonoBehaviour
{
    [SerializeField]
    Transform destination;

    public void Update()
    {
        if (destination == null)
            return;
        GetComponent<NavMeshAgent>().SetDestination(destination.position);
    }
}
