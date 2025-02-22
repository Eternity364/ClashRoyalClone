
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Experimental.AI;

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
    [SerializeField]    
    private Transform child;

    public float AttackRange => navMeshAgent.stoppingDistance;
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
    private float rotationSpeed = 10f;
    private Tween rotationTween;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalColor = ren.material.color;
        speed = navMeshAgent.speed;
        navMeshAgent.updateRotation = false;
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
        RotateTowards(attackTarget.transform);
    } 
    
    private void ClearAttackTarget(Dude _) {
        if (attackTarget != null) {
            attackTarget.OnDeath -= ClearAttackTarget;
            attackTarget = null;
        }
        navMeshAgent.isStopped = false;
        RotateTowards(null);
    } 

    private void ReceiveAttack(int damage) {
        health -= damage;
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

    private void RotateTowards(Transform target) {
        if (rotationTween != null)
            rotationTween.Kill(false);
        if (target == null) {
            rotationTween = child.DOLocalRotate(Vector3.zero, 1);
        }
        else
        {
            Vector3 worldUp = new Vector3(0, 0, -transform.position.z);
            Vector3 originalRotation = child.localEulerAngles;
            child.DOLookAt(target.position, 0, AxisConstraint.Z, worldUp).OnComplete(OnComplete);

            void OnComplete() {
                Vector3 newRotation = child.localEulerAngles;
                print("newRotation = " + newRotation);
                child.localEulerAngles = originalRotation;
                rotationTween = child.DOLocalRotate(newRotation, 1);
            }
        }
    }
}
