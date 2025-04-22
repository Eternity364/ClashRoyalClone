
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using RPGCharacterAnims;
using Unity.VisualScripting;
using System.Collections;
using System;
using RPGCharacterAnims.Actions;


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
    [SerializeField]    
    private GameObject arrow;

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
    private bool isDead = false;
    
    private RPGCharacterController rPGCharacterController;

    public void Start() {
        originalColor = ren.material.color;
      
        rPGCharacterController = GetComponent<RPGCharacterController>();
        rPGCharacterController.enabled = true;

        SwitchWeaponContext context = new SwitchWeaponContext();
        //Switch to unarmed.
        context.type = "Unsheath";
        context.side = "None";
        context.sheathLocation = "Back";
        context.leftWeapon = -1;
        context.rightWeapon = (int)Weapon.RightSword;
        rPGCharacterController.StartAction("SwitchWeapon", context);
    }   

    public void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) {
        this.bulletFactory = bulletFactory;
        this.destination = destination;
        this.team = team;
        timePassedSinceLastAttack = attackRate;
        health = maxHealth;
        ren.material.SetColor("_TeamColor", teamColor);
        isDead = false;
        
        rPGCharacterController = GetComponent<RPGCharacterController>();
        rPGCharacterController.enabled = true;
        
        SwitchWeaponContext context = new SwitchWeaponContext();
        //Switch to unarmed.
        context.type = "Instant";
        context.side = "None";
        context.sheathLocation = "Back";
        context.leftWeapon = -1;
        context.rightWeapon = (int)Weapon.TwoHandBow;
        rPGCharacterController.StartAction("SwitchWeapon", context);

        navMeshAgent.enabled = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.SetDestination(destination.position);
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
        StartCoroutine(nameof(PositionChecker));
        attackAllowed = false;
    }
    
    /* Because after switching from NavMeshObstacle to NavMeshAgent back again, the position of the unit slightly changes,
        so we need to set it to one from the frame before (on current frame it's the same for some reason).*/
    private IEnumerator PositionChecker() {
        yield return new WaitForNextFrameUnit();
        if (isDead)
            yield break;
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
        animator.SetBool("Moving", isMoving);
        animator.SetFloat("Velocity Z", isMoving ? 1 : 0);
    }

    private void TriggerBowAnimation(Action OnComplete) {
        animator.SetBool("Aiming", true);
        arrow.SetActive(true);

        void SetBowPullValue(float value) {
            animator.SetFloat("BowPull", value);
        }

        DOTween.To(SetBowPullValue, 0, 1, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => { 
            DOTween.To(SetBowPullValue, 1, 0, 0.25f).SetEase(Ease.InCubic).OnComplete(() => {
                animator.SetBool("Aiming", false);
            });

            arrow.SetActive(false);
            OnComplete();
        });
    } 

    private void PerformDeath() {
        if (isDead)
            return;
        
        isDead = true;
        navMeshAgent.enabled = false;
        navMeshObstacle.enabled = false;
        attackAllowed = false;
        attackTarget = null;

        if (rotationAnimation != null) {
            rotationAnimation.Kill(false);
            rotationAnimation = null;
        }
        OnDeath?.Invoke(this);
        OnDeath = null;

        rPGCharacterController.StartAction("Death");

        DG.Tweening.Sequence deathSeq = DOTween.Sequence();
        deathSeq.Insert(1f, transform.DOBlendableMoveBy(new Vector3(0, -2, 0), 2f));
        deathSeq.InsertCallback(3f, () => {
            ObjectPool.Instance.ReturnObject(gameObject);
            rPGCharacterController.EndAction("Death");
        });
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
        if (attackTarget != null) {
            GameObject bullet = bulletFactory.Get();
            bullet.transform.localScale = arrow.transform.lossyScale;
            ArrowFlight arrowFlight = bullet.GetComponent<ArrowFlight>();
            NavMeshAgent attackTargetNavMesh = attackTarget.GetComponent<NavMeshAgent>();
            float attackTargetSize = attackTargetNavMesh.radius * attackTarget.transform.lossyScale.x;
            Vector3 targetPosition = attackTarget.transform.position;

            TriggerBowAnimation(() => {
                bullet.SetActive(true);
                arrowFlight.FlyArrow(arrow.transform.position, targetPosition, attackTargetSize, OnBulletFlyComplete);
            });

            void OnBulletFlyComplete() {
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
        rotationAnimation.Append(transform.DORotate(new Vector3(0, angle, 0), Math.Abs(angle) / navMeshAgent.angularSpeed));
    }
}
