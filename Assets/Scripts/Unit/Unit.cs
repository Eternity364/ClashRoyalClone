
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using RPGCharacterAnims;
using Unity.VisualScripting;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Units{
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField]
        protected UnitData data;
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
        private List<Renderer> renderers;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected float checkForAttackTargetRate = 0.1f;

        /// <summary>
        /// Range, on which unit spots an enemy and starts walking to it. Must be equal or greater than attack range.
        /// </summary>
        public float AttackNoticeRange => data.AttackNoticeRange;
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
        public float Radius
        {
            get
            {
                return navMeshAgent.radius;
            }
        }
        public UnitData Data
        {
            get
            {
                return data;
            }
        }
        public UnityAction<Unit> OnDeath;

        protected Transform destination;
        protected Unit attackTarget;
        protected int team;
        protected float timePassedSinceLastAttack = 0;
        protected float timePassedSinceLastAttackTargetCheck = 0;
        protected Color originalColor;
        protected DG.Tweening.Sequence damageAnimation;
        protected DG.Tweening.Sequence rotationAnimation;
        protected bool attackAllowed = false;
        protected bool attackTargetFound = false;
        protected Vector3 positionBefore;
        protected int health;
        protected bool isDead = true;

        protected RPGCharacterController rPGCharacterController;

        public void Awake()
        {
            originalColor = ren.material.color;
            rPGCharacterController = GetComponent<RPGCharacterController>();
            if (rPGCharacterController != null)
                rPGCharacterController.enabled = true;
        }

        public virtual void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor)
        {
            isDead = false;
            this.bulletFactory = bulletFactory;
            this.destination = destination;
            this.team = team;
            health = data.MaxHealth;
            SetTeamColor(teamColor);

            navMeshAgent.enabled = true;
            navMeshAgent.updateRotation = true;
            navMeshAgent.SetDestination(destination.position);
            StartMovingAnimation(true);

            if (data.AttackNoticeRange >= data.AttackRange)
            {
                Debug.LogError("Attack notice range must be equal or greater than attack range.");
            }
        }

        public virtual void SetAttackTarget(Unit unit)
        {
            if (attackTarget != null)
            {
                attackTarget.OnDeath -= ClearAttackTarget;
                attackTarget = null;
            }
            attackTarget = unit;
            attackTarget.OnDeath += ClearAttackTarget;
            attackAllowed = false;
            attackTargetFound = true;
            timePassedSinceLastAttackTargetCheck = checkForAttackTargetRate;
        }

        public void ReceiveAttack(int damage)
        {
            health -= damage;
            StartDamageAnimation();
            if (health <= 0)
                PerformDeath();
        }

        public void SetTeamColor(Color color)
        {
            ren.material.SetColor("_TeamColor", color);
        }

        public void SetAlpha(float value)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material.SetColor("_Color", new Color(1, 1, 1, value));
            }
        }

        protected virtual void CheckIfAttackTargetReachable()
        {
            if (navMeshObstacle.isActiveAndEnabled || navMeshAgent.isOnNavMesh)
            {
                float distance = (transform.position - attackTarget.transform.position).magnitude - attackTarget.Radius;
                if (distance > data.AttackNoticeRange)
                {
                    attackTargetFound = false;
                    attackTarget.OnDeath -= ClearAttackTarget;
                    ClearAttackTarget(attackTarget);
                }
                else if (distance <= data.AttackNoticeRange)
                {
                    if (distance <= data.AttackRange)
                    {
                        attackTargetFound = false;
                        StartMovement(false, Vector3.zero);
                        RotateTowards(attackTarget.transform, OnAttackRotationComplete);
                    }
                    else
                    {
                        StartMovement(true, attackTarget.transform.position);
                    }
                }
            }
        }

        protected virtual void ClearAttackTarget(Unit unit)
        {
            if (isDead)
                return;

            attackTarget = null;

            StartMovement(true, destination.position);

            attackAllowed = false;
            attackTargetFound = false;
        }

        protected void StartMovement(bool isMoving, Vector3 destPos)
        {
            positionBefore = transform.position;
            navMeshObstacle.enabled = !isMoving;
            navMeshAgent.enabled = isMoving;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = !isMoving;
                navMeshAgent.updateRotation = isMoving;
            }
            StartMovingAnimation(isMoving);

            if (isMoving)
                StartCoroutine(nameof(SetDestination), destPos);
        }

        /* Because after switching from NavMeshObstacle to NavMeshAgent back again, the position of the unit slightly changes,
            so we need to set it to one from the frame before (on current frame it's the same for some reason).*/
        protected IEnumerator SetDestination(Vector3 destPos)
        {
            yield return new WaitForNextFrameUnit();
            if (isDead)
                yield break;
            transform.position = positionBefore;
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.SetDestination(destPos);
        }

        protected void StartMovingAnimation(bool isMoving)
        {
            animator.SetBool("Moving", isMoving);
            animator.SetFloat("Velocity Z", isMoving ? 1 : 0);
        }

        protected void PerformDeath()
        {
            if (isDead)
                return;

            navMeshAgent.enabled = false;
            navMeshObstacle.enabled = false;
            attackAllowed = false;
            attackTarget = null;

            if (rotationAnimation != null)
            {
                rotationAnimation.Kill(false);
                rotationAnimation = null;
            }
            OnDeath?.Invoke(this);
            OnDeath = null;

            rPGCharacterController.StartAction("Death");

            DG.Tweening.Sequence deathSeq = DOTween.Sequence();
            deathSeq.Insert(1f, transform.DOBlendableMoveBy(new Vector3(0, -2, 0), 2f));
            deathSeq.InsertCallback(3f, () =>
            {
                rPGCharacterController.EndAction("Death");
                ObjectPool.Instance.ReturnObject(gameObject);
            });

            isDead = true;
        }

        private void StartDamageAnimation()
        {
            if (damageAnimation != null)
            {
                damageAnimation.Kill();
            }
            damageAnimation = DOTween.Sequence();
            damageAnimation.Append(ren.material.DOColor(damageColor, 0.1f));
            damageAnimation.Append(ren.material.DOColor(originalColor, 0.3f));
        }

        protected virtual void PerformAttack()
        {
            if (isDead)
                return;
        }

        private void Update()
        {
            if (isDead)
                return;
            if (attackTarget != null)
            {
                if (attackTargetFound)
                {
                    timePassedSinceLastAttackTargetCheck += Time.deltaTime;
                    if (timePassedSinceLastAttackTargetCheck >= checkForAttackTargetRate)
                    {
                        timePassedSinceLastAttackTargetCheck = -checkForAttackTargetRate;
                        CheckIfAttackTargetReachable();
                    }
                }
                else if (attackAllowed)
                {
                    timePassedSinceLastAttack += Time.deltaTime;
                    if (timePassedSinceLastAttack >= data.AttackRate)
                    {
                        timePassedSinceLastAttack = -data.AttackRate;
                        PerformAttack();
                    }
                }
            }
        }

        protected virtual void OnAttackRotationComplete()
        {
            StartAttacking();
        }

        protected virtual void StartAttacking()
        {
            if (attackAllowed)
                return;
            attackAllowed = true;
            timePassedSinceLastAttack = data.AttackRate;
        }

        private void RotateTowards(Transform target, TweenCallback OnComplete = null)
        {
            if (rotationAnimation != null)
            {
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