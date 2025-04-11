
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using RPGCharacterAnims;
using Unity.VisualScripting;
using System.Collections;
using System;

public class Unit : MonoBehaviour
{    

    [SerializeField]
    int maxHealth;
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
    private float bulletSpeed;
    [SerializeField]    
    private BulletFactory bulletFactory;
    [SerializeField]    
    private NavMeshAgent navMeshAgent;
    [SerializeField]    
    private NavMeshObstacle navMeshObstacle;
    [SerializeField]    
    private Animator animator;

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

    public Transform destination;
    private Unit attackTarget;
    private float timePassedSinceLastAttack = 0;
    private Color originalColor;
    private DG.Tweening.Sequence damageAnimation;
    private DG.Tweening.Sequence rotationAnimation;
    private bool attackAllowed = false;
    private Vector3 positionBefore;
    private int health;

    public void Start() {
        // RPGCharacterController controller = GetComponent<RPGCharacterController>();
        // if (controller != null) {
        //     controller.StartAction("Sprint");
        // };
    }   

    public void Init(Transform destination, BulletFactory bulletFactory, int navMeshPriority) {
        this.bulletFactory = bulletFactory;
        this.destination = destination;
        timePassedSinceLastAttack = attackRate;
        health = maxHealth;
        navMeshAgent.enabled = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.SetDestination(destination.position);
        animator.SetBool("Moving", true);
        StartMovingAnimation(true);
}   

    public void SetAttackTarget(Unit unit) {
        if (attackTarget != null) {
            attackTarget.OnDeath -= ClearAttackTarget;
            attackTarget = null;
        }
        attackAllowed = false;
        navMeshAgent.isStopped = true;
        navMeshAgent.updateRotation = false;
        navMeshAgent.enabled = false;
        StartMovingAnimation(false);
        navMeshObstacle.enabled = true;
        attackTarget = unit;
        attackTarget.OnDeath += ClearAttackTarget;
        RotateTowards(unit.transform, OnComplete);

        void OnComplete () {
            attackAllowed = true;
        }
    } 
    
    private void ClearAttackTarget(Unit _) {
        attackTarget = null;
        //RotateTowards(null);
        positionBefore = transform.position;
        navMeshObstacle.enabled = false;
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = true;
        StartMovingAnimation(true);
        //print("navMeshAgent.destination " + this.name + " " + destination.position);
        StartCoroutine("PositionChecker");
        attackAllowed = false;
    }
    
    /* Because after switching from NavMeshObstacle to NavMeshAgent back again, the position of the unit slightly changes,
        so we need to set it to one from the frame before (on current frame it's the same for some reason).
    */
    private IEnumerator PositionChecker() {
        yield return new WaitForNextFrameUnit();
        transform.position = positionBefore;
        navMeshAgent.SetDestination(destination.position);
    }

    private void ReceiveAttack(int damage) {
        health -= damage;
        StartDamageAnimation();
        if (health <= 0)
            PerformDeath();
    } 
    private void StartMovingAnimation(bool isMoving) {
        animator.SetFloat("Velocity Z", isMoving ? 1 : 0);
    } 

    private void PerformDeath() {
        if (this.IsDestroyed())
            return;
        ClearAttackTarget(null);
        navMeshAgent.isStopped = true;
        if (rotationAnimation != null) {
            rotationAnimation.Kill(false);
            rotationAnimation = null;
        }
        OnDeath?.Invoke(this);
        OnDeath = null;
        ObjectPool.Instance.ReturnObject(gameObject);
    }

    private void StartDamageAnimation() {
        // if (damageAnimation != null) {
        //     damageAnimation.Kill();
        // }
        // damageAnimation = DOTween.Sequence();
        // damageAnimation.Append(ren.material.DOColor(damageColor, 0.1f));
        // damageAnimation.Append(ren.material.DOColor(originalColor, 0.3f));
    }

    private void PerformAttack() {
        GameObject bullet = bulletFactory.Get();
        bullet.transform.position = transform.position;
        bullet.SetActive(true);
        if (attackTarget != null) {
            Vector3 targetPosition = attackTarget.transform.position;
            float duration = (transform.position - targetPosition).magnitude / bulletSpeed;
            DG.Tweening.Sequence mySequence = DOTween.Sequence();
            mySequence.Append(bullet.transform.DOMove(targetPosition, duration));
            mySequence.InsertCallback(duration * 0.8f, OnComplete);
            mySequence.SetAutoKill(true);


            void OnComplete() {
                bullet.SetActive(false);
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
        if (rotationAnimation != null) {
            rotationAnimation.Kill(false);
            rotationAnimation = null;
        }

        rotationAnimation = DOTween.Sequence();
        rotationAnimation.OnComplete(OnComplete);
        float angle = Mathf.Atan2(target.position.x - transform.position.x, target.position.z - transform.position.z) * Mathf.Rad2Deg;
        print("angle: " + angle);
        rotationAnimation.Append(transform.DORotate(new Vector3(0, angle, 0), Math.Abs(angle) / navMeshAgent.angularSpeed));
    }
}
