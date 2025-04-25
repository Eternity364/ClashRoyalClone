
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using RPGCharacterAnims;
using Unity.VisualScripting;
using System.Collections;
using System;

namespace Assets.Scripts.Unit {
    public abstract class Unit : MonoBehaviour
    {    

        [SerializeField]
        protected int maxHealth;
        [SerializeField]
        protected int attack;
        [SerializeField]
        protected float attackRange;
        [SerializeField]
        protected float attackRate;
        [SerializeField]
        protected int team;
        [SerializeField]
        protected Color damageColor;
        [SerializeField]    
        protected Renderer ren;
        [SerializeField]    
        protected BulletFactory bulletFactory;
        [SerializeField]    
        protected NavMeshAgent navMeshAgent;
        [SerializeField]    
        protected NavMeshObstacle navMeshObstacle;
        [SerializeField]    
        protected Animator animator;

        public float AttackRange => attackRange;
        public int Team => team;
        public bool IsDead
        {
            get
            {
                return isDead;
            }
        }
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
        protected Unit attackTarget;
        protected float timePassedSinceLastAttack = 0;
        protected Color originalColor;
        protected DG.Tweening.Sequence damageAnimation;
        protected DG.Tweening.Sequence rotationAnimation;
        protected bool attackAllowed = false;
        protected Vector3 positionBefore;
        protected int health;
        protected bool isDead = false;
        
        protected RPGCharacterController rPGCharacterController;

        public void Awake() {
            originalColor = ren.material.color;
            rPGCharacterController = GetComponent<RPGCharacterController>();
            rPGCharacterController.enabled = true;
        }   

        public virtual void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) {
            this.bulletFactory = bulletFactory;
            this.destination = destination;
            this.team = team;
            timePassedSinceLastAttack = attackRate;
            health = maxHealth;
            ren.material.SetColor("_TeamColor", teamColor);
            isDead = false;

            navMeshAgent.enabled = true;
            navMeshAgent.updateRotation = true;
            navMeshAgent.SetDestination(destination.position);
            StartMovingAnimation(true);
        }   

        public virtual void SetAttackTarget(Unit unit) {
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

        public void ReceiveAttack(int damage) {
            health -= damage;
            StartDamageAnimation();
            if (health <= 0)
                PerformDeath();
        } 
        
        protected virtual void ClearAttackTarget(Unit unit) {
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
        protected IEnumerator PositionChecker() {
            yield return new WaitForNextFrameUnit();
            if (isDead)
                yield break;
            transform.position = positionBefore;
            navMeshAgent.SetDestination(destination.position);
        }

        protected void StartMovingAnimation(bool isMoving) {
            animator.SetBool("Moving", isMoving);
            animator.SetFloat("Velocity Z", isMoving ? 1 : 0);
        }

        protected void PerformDeath() {
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

        protected abstract void PerformAttack();

        private void Update() {
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
}