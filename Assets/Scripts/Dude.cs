
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;

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
    [SerializeField]
    private Color damageColor;
    [SerializeField]    
    private Renderer ren;

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
    [SerializeField]    
    private Transform destination;
    private Dude attackTarget;
    private float timePassedSinceLastAttack = 0;
    private Color originalColor;
    private Sequence damageAnimation;
    private float speed;
    private float multiplier = 10;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalColor = ren.material.color;
        speed = navMeshAgent.speed;
    }

    public void Init(Transform destination) {
        this.destination = destination;
        navMeshAgent.destination = destination.position;
    }   

    public void SetAttackTarget(Dude dude) {
        attackTarget = dude;
        attackTarget.OnDeath += ClearAttackTarget;
        timePassedSinceLastAttack = 0;
        navMeshAgent.isStopped = true;
        transform.DOLookAt(attackTarget.transform.position, 0.5f, AxisConstraint.Z, Vector3.forward);
        //transform.LookAt(attackTarget.transform.position, Vector3.forward);
    } 
    
    private void ClearAttackTarget(Dude _) {
        if (attackTarget != null) {
            attackTarget.OnDeath -= ClearAttackTarget;
            attackTarget = null;
        }
        navMeshAgent.isStopped = false;
    } 

    private void ReceiveAttack(int damage) {
        health -= damage;
        print("Health = " + health);
        StartAttackAnimation();
        if (health <= 0)
            PerformDeath();
    } 

    private void ReceiveAttackAnimation(int damage) {
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
        if (damageAnimation != null) {
            damageAnimation.Kill();
        }
        damageAnimation = DOTween.Sequence();
        damageAnimation.Append(ren.material.DOColor(damageColor, 0.3f));
        damageAnimation.Append(ren.material.DOColor(originalColor, 0.3f));
    }

    private void PerformAttack() {
        attackTarget.ReceiveAttack(attack);
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
