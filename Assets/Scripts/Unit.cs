
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class Unit : MonoBehaviour
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
    [SerializeField]    
    private GameObject bulletPrefab;
    [SerializeField]    
    private NavMeshAgent navMeshAgent;

    public float AttackRange => attackRange;
    public int Team => team;

    public bool HasTarget
    {
        get
        {
            return attackTarget != null;
        }
    }
    public Unit Target
    {
        get
        {
            return attackTarget;
        }
    }
    public UnityAction<Unit> OnDeath;

    [SerializeField]    
    private Transform destination;
    private Unit attackTarget;
    private float timePassedSinceLastAttack = 0;
    private Color originalColor;
    private DG.Tweening.Sequence damageAnimation;
    private float speed;
    private float rotationSpeed = 10f;
    private DG.Tweening.Sequence rotationAnimation;
    private bool attackAllowed = false;

    void Awake() {
        originalColor = ren.material.color;
        speed = navMeshAgent.speed;
    }

    public void Init(Transform destination) {
        this.destination = destination;
        navMeshAgent.destination = destination.position;
        timePassedSinceLastAttack = attackRate;
    }   

    public void SetAttackTarget(Unit unit) {
        ClearAttackTarget(null);
        navMeshAgent.isStopped = true;
        navMeshAgent.updateRotation = false;
        attackTarget = unit;
        attackTarget.OnDeath += ClearAttackTarget;
        RotateTowards(unit.transform, OnComplete);

        void OnComplete () {
            attackAllowed = true;
        }
    } 
    
    private void ClearAttackTarget(Unit _) {
        if (attackTarget != null) {
            attackTarget.OnDeath -= ClearAttackTarget;
            attackTarget = null;
        }
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = true;
        attackAllowed = false;
        RotateTowards(null);
    } 

    private void ReceiveAttack(int damage) {
        health -= damage;
        StartDamageAnimation();
        if (health <= 0)
            PerformDeath();
    } 

    private void PerformDeath() {
        ClearAttackTarget(null);
        navMeshAgent.isStopped = true;
        if (rotationAnimation != null)
            rotationAnimation.Kill(false);
        OnDeath?.Invoke(this);
        UnityEngine.Object.Destroy(gameObject);
    }

    private void StartDamageAnimation() {
        if (damageAnimation != null) {
            damageAnimation.Kill();
        }
        damageAnimation = DOTween.Sequence();
        damageAnimation.Append(ren.material.DOColor(damageColor, 0.1f));
        damageAnimation.Append(ren.material.DOColor(originalColor, 0.3f));
    }

    private void PerformAttack() {
        GameObject bullet = Instantiate(bulletPrefab, transform.parent);
        bullet.transform.position = transform.position;
        if (attackTarget != null) {
            Vector3 targetPosition = attackTarget.transform.position;
            float duration = 1;
            DG.Tweening.Sequence mySequence = DOTween.Sequence();
            mySequence.Append(bullet.transform.DOMove(targetPosition, duration));
            mySequence.InsertCallback(duration * 0.8f, OnComplete);
            mySequence.SetAutoKill(true);


            void OnComplete() {
                GameObject.Destroy(bullet);
                if (attackTarget != null)
                    attackTarget.ReceiveAttack(attack);
            }
        }
    }

    void Update() {
        if (attackTarget != null && attackAllowed) {
            timePassedSinceLastAttack += Time.deltaTime;
            if (timePassedSinceLastAttack >= attackRate) {
                timePassedSinceLastAttack =- attackRate;
                PerformAttack();
            }
        }
    }

    private void RotateTowards(Transform target, TweenCallback OnComplete = null) {
        if (rotationAnimation != null)
            rotationAnimation.Kill(false);
        rotationAnimation = DOTween.Sequence();
        if (target == null) {
            rotationAnimation.Append(child.DOLocalRotate(Vector3.zero, 1));
        }
        else
        {
            rotationAnimation.OnComplete(OnComplete);
            Vector3 worldUp = new Vector3(0, 0, -transform.position.z);
            Vector3 originalchildRotation = child.localEulerAngles;
            child.DOLookAt(target.position, 0, AxisConstraint.Z, worldUp).OnComplete(OnCompleteLocal);

            void OnCompleteLocal() {
                Vector3 newRotation = child.localEulerAngles;
                child.localEulerAngles = originalchildRotation;                
                rotationAnimation.Append(child.DOLocalRotate(newRotation, 1));
            }
        }
    }
}
