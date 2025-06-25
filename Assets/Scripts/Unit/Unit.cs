
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
    public enum AllowedTargets
    {
        All,
        Base
    }

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
        public bool IsDead => isDead;
        public bool HasTarget => attackTarget != null;
        public Unit Target => attackTarget;
        public AllowedTargets AllowedTargets => allowedTargets;
        public float Radius => navMeshAgent.radius;
        public UnitData Data => data;
        public UnityAction<Unit> OnDeath;

        protected Transform destination;
        protected Unit attackTarget;
        protected int team;
        protected float timePassedSinceLastAttack = 0;
        protected float timePassedSinceLastAttackTargetCheck = 0;
        protected float deathAnimationDepth = 2;
        protected Color originalColor;
        protected DG.Tweening.Sequence damageAnimation;
        protected DG.Tweening.Sequence rotationAnimation;
        protected bool attackAllowed = false;
        protected bool attackTargetFound = false;
        protected bool mandatoryFirstAttack = false;
        protected Vector3 positionBefore;
        protected int health;
        protected bool isDead = true;
        protected AllowedTargets allowedTargets = AllowedTargets.All;
        protected RPGCharacterController rPGCharacterController;

        protected virtual void Awake()
        {
            originalColor = ren.material.color;
            rPGCharacterController = GetComponent<RPGCharacterController>();
            if (rPGCharacterController != null)
                rPGCharacterController.enabled = true;
        }

        protected virtual void Start()
        {
        }

        public virtual void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor)
        {
            isDead = false;
            this.bulletFactory = bulletFactory;
            this.destination = destination;
            this.team = team;
            health = data.MaxHealth;
            timePassedSinceLastAttack = data.AttackRate;
            SetTeamColor(teamColor);
            InitNavMesh();
            GetComponent<Collider>().enabled = true;

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
            timePassedSinceLastAttackTargetCheck = 0;
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
                Color color = renderers[i].material.color;
                color.a = value;
                renderers[i].material.color = color;
            }
        }

        public void SetTransparent(bool enabled)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (!enabled) // Opaque
                {
                    renderers[i].material.SetOverrideTag("RenderType", "Opaque");
                    renderers[i].material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    renderers[i].material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    renderers[i].material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    renderers[i].material.SetInt("_ZWrite", 1);
                }
                else // Transparent
                {
                    renderers[i].material.SetOverrideTag("RenderType", "Transparent");
                    renderers[i].material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    renderers[i].material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    renderers[i].material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    renderers[i].material.SetInt("_ZWrite", 0);
                }
            }
        }

        public void SetEmissionStrenght(float value)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material.SetFloat("_EmissionStrength", value);
            }
        }

        private void InitNavMesh()
        {
            navMeshAgent.enabled = true;
            navMeshAgent.updateRotation = true;
            StartMovement(true, destination.position);
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
                        //attackTargetFound = false;
                        if (navMeshAgent.enabled)
                        {
                            StartMovement(false, Vector3.zero);
                            mandatoryFirstAttack = true;
                        }
                        RotateTowards(attackTarget.transform);
                        StartAttacking();
                    }
                    else
                    {
                        attackAllowed = false;
                        StartMovement(true, attackTarget.transform.position);
                    }
                }
            }
        }

        protected bool IsTargetInAttackRange()
        {
            float distance = (transform.position - attackTarget.transform.position).magnitude - attackTarget.Radius;
            return distance <= data.AttackRange;
        }

        protected virtual void ClearAttackTarget(Unit unit)
        {
            if (isDead)
                return;

            attackTarget = null;

            StartMovement(true, destination.position);

            attackAllowed = false;
            attackTargetFound = false;
            mandatoryFirstAttack = false;
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

        protected virtual void StartMovingAnimation(bool isMoving)
        {
            animator.SetBool("Moving", isMoving);
            animator.SetFloat("Velocity Z", isMoving ? 1 : 0);
        }

        protected virtual void PerformDeath()
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
            transform.localRotation = Quaternion.identity;
            GetComponent<Collider>().enabled = false;

            rPGCharacterController.EndAction("Death");
            rPGCharacterController.StartAction("Death");

            DG.Tweening.Sequence deathSeq = DOTween.Sequence();
            deathSeq.Insert(1f, transform.DOBlendableMoveBy(new Vector3(0, -deathAnimationDepth, 0), 2f));
            deathSeq.InsertCallback(3f, () =>
            {
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
            damageAnimation.Append(ren.material.DOColor(damageColor, 0.2f).SetEase(Ease.InQuad));
            damageAnimation.Append(ren.material.DOColor(originalColor, 0.15f).SetEase(Ease.OutQuad));
        }

        protected virtual void PerformAttack(TweenCallback OnFinish)
        {
            if (isDead)
                return;
        }

        private void Update()
        {
            if (isDead)
                return;
            
            timePassedSinceLastAttack += Time.deltaTime;

            if (attackTarget != null)
            {
                if (attackTargetFound)
                {
                    timePassedSinceLastAttackTargetCheck += Time.deltaTime;
                    if (timePassedSinceLastAttackTargetCheck >= checkForAttackTargetRate && !mandatoryFirstAttack)
                    {
                        timePassedSinceLastAttackTargetCheck = -checkForAttackTargetRate;
                        CheckIfAttackTargetReachable();
                    }
                    if (attackAllowed)
                    {
                        if (timePassedSinceLastAttack >= data.AttackRate)
                        {
                            timePassedSinceLastAttack = 0;
                            PerformAttack(() =>
                            {
                                mandatoryFirstAttack = false;
                            });
                        }
                    }
                }
            }
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
            float angle = Mathf.Atan2(target.position.x - transform.position.x, target.position.z - transform.position.z) * Mathf.Rad2Deg;
            rotationAnimation.Append(transform.DORotate(new Vector3(0, angle, 0), Math.Abs(angle) / navMeshAgent.angularSpeed));
            if (OnComplete != null)
                rotationAnimation.OnComplete(OnComplete);
        }
    }
}