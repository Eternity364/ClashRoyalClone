using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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

    public float AttackRange => attackRange;
    public int Team => team;

    public bool HasTarget
    {
        get
        {
            return attackTarget != null;
        }
    }
    public UnityAction<Dude> OnDeath;

    private NavMeshAgent navMeshAgent;
    private Transform destination;
    private Dude attackTarget;
    private float timePassedSinceLastAttack = 0;

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
        attackTarget.OnDeath += ClearAttackTarget;
        transform.LookAt(attackTarget.transform);
        timePassedSinceLastAttack = 0;
    } 
    
    public void ClearAttackTarget(Dude _) {
        if (attackTarget != null) {
            attackTarget.OnDeath -= ClearAttackTarget;
            attackTarget = null;
        }
        navMeshAgent.isStopped = false;
    } 

    public void ReceiveAttack(int damage) {
        health -= damage;
        StartAttackAnimation();
        if (health <= 0)
            PerformDeath();
    } 

    private void PerformDeath() {
        ClearAttackTarget(null);
        navMeshAgent.isStopped = true;
        OnDeath?.Invoke(this);
        UnityEngine.Object.Destroy(gameObject);
    }

    private void StartAttackAnimation() {
    }

    private void PerformAttack() {
        attackTarget.ReceiveAttack(attack);
        print("attack");
    }

    void Update() {
        if (attackTarget != null) {
            timePassedSinceLastAttack += Time.deltaTime;
            if (timePassedSinceLastAttack > attackRate) {
                timePassedSinceLastAttack =- attackRate;
                PerformAttack();
            }
        }
    }
}
