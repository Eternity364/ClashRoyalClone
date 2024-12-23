using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dude : MonoBehaviour
{    
    [SerializeField]
    int health;
    [SerializeField]
    int attack;
    [SerializeField]
    float attackRange;
    [SerializeField]
    float attackRate;
    [SerializeField]
    private int team;
    private NavMeshAgent navMeshAgent;
    private Transform destination;
    private Dude attackTarget;
    private float timePassedSinceLastAttack;

    public float AttackRange => attackRange;
    public int Team => team;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Init(Transform destination) {
        this.destination = destination;
        navMeshAgent.destination = destination.position;
    }   

    public void SetAttackTarget(Dude dude) {
        attackTarget = dude;
        navMeshAgent.isStopped = true;
    }

    // void Update() {
    //     navMeshAgent.destination = destination.position;
    // }
}
