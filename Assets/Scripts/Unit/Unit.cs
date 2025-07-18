
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
    
    public enum Size
    {
        Big,
        Medium,
        Small
    }

    public enum Type
    {
        Base,
        Swordsman,
        Ranged,
        Giant,
        MiniSkeleton
    }

    public abstract class Unit : MonoBehaviour, ISpawnable
    {

        [SerializeField]
        protected UnitData data;
        [SerializeField]
        protected Color damageColor;
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
        [SerializeField]
        protected Type type;

        public virtual ISpawnable Spawnable => this;
        /// <summary>
        /// Range, on which unit spots an enemy and starts walking to it. Must be equal or greater than attack range.
        /// </summary>
        public float AttackNoticeRange => data.AttackNoticeRange;
        public int Team => team;
        public bool IsDead => isDead;
        public bool HasTarget => attackTarget != null;
        public Unit Target => attackTarget;
        public float baseOffset => navMeshAgent.baseOffset;
        public AllowedTargets AllowedTargets => allowedTargets;
        public float Radius => navMeshAgent.radius;
        public UnitData Data => data;
        public UnityAction<Unit> OnDeath;
        public event UnityAction<Color> OnTeamColorSet;
        public event UnityAction<float> OnEmissionStrengthSet;
        public Type Type => type;

        protected Transform destination;
        protected Unit attackTarget;
        protected int team;
        protected float timePassedSinceLastAttack = 0;
        protected float timePassedSinceLastAttackTargetCheck = 0;
        protected float deathAnimationDepth = 2;
        protected Color originalColor = Color.white;
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
            rPGCharacterController = GetComponent<RPGCharacterController>();
            if (rPGCharacterController != null)
                rPGCharacterController.enabled = true;
        }

        public virtual void Init(Transform destination, int team)
        {
            this.team = team;
            health = data.MaxHealth;

            if (this.GetType() == typeof(Base))
                return;

            isDead = false;
            this.destination = destination;
            timePassedSinceLastAttack = data.AttackRate;
            InitNavMesh();
            GetComponent<Collider>().enabled = true;

            if (data.AttackNoticeRange < data.AttackRange)
            {
                Debug.LogError("Attack notice range must be equal or greater than attack range.");
            }
        }

        public void PerformActionForEachUnit(Action<Unit> Action)
        {
            Action(this);
        }

        public virtual GameObject GetGameObject()
        {
            return gameObject;
        }

        public virtual void SetAttackTarget(Unit unit, bool overrideMandatoryFirstAttack = false)
        {
            if (isDead)
                return;

            if (attackTarget != null)
            {
                attackTarget.OnDeath -= ClearAttackTarget;
            }

            if (overrideMandatoryFirstAttack)
            {
                mandatoryFirstAttack = false;
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
            DoActionForAllMaterials(mat =>
            {
                mat.SetColor("_TeamColor", color);
            });
            OnTeamColorSet?.Invoke(color);
        }

        public void SetCopyMode(bool enabled)
        {
            SetTransparent(enabled);
            SetShadowCastingMode(!enabled);
            if (enabled)
            {
                SetAlpha(0.4f);
                SetTeamColor(Color.white);
            }
        }

        public void SetEmissionStrength(float value)
        {
            DoActionForAllMaterials(mat =>
            {
                mat.SetFloat("_EmissionStrength", value);
            });
            OnEmissionStrengthSet?.Invoke(value);
        }

        public void Release(bool destroyChildren)
        {   
            if (destroyChildren)
                ObjectPool.Instance.ReturnObject(gameObject);
        }

        public void SetParentForUnits(Transform parent)
        {
            if (parent != transform.parent)
                transform.SetParent(parent);
        }

        protected virtual void OnEnable()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
        }

        private void SetAlpha(float value)
        {
            DoActionForAllMaterials(mat =>
            {
                Color color = mat.color;
                color.a = value;
                mat.color = color;
            });
        }

        private void SetTransparent(bool enabled)
        {
            DoActionForAllMaterials(mat =>
            {
                if (enabled)
                {
                    mat.SetFloat("_Mode", 2f);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                }
                else
                {
                    mat.SetFloat("_Mode", 0f);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                }
            });
        }


        private void SetShadowCastingMode(bool enabled)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (enabled)
                {
                    renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                else
                {
                    renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
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
            attackAllowed = false;
            attackTargetFound = false;
            mandatoryFirstAttack = false;

            StartMovement(true, destination.position);
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
            mandatoryFirstAttack = false;
            attackTargetFound = false;

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
            DoActionForAllMaterials(mat =>
            {
                damageAnimation.Append(mat.DOColor(damageColor, 0.1f).SetEase(Ease.InSine));
                damageAnimation.Append(mat.DOColor(originalColor, 0.1f).SetEase(Ease.OutSine));
            });
        }

        protected virtual void PerformAttack(TweenCallback OnFinish)
        {
            if (isDead)
                return;
        }

        private void Update()
        {
            // if (this is not Base)
            //     print("Rotation = " + transform.localEulerAngles.y);
            if (isDead)
                return;


            if (attackTarget != null && attackTargetFound)
            {
                timePassedSinceLastAttackTargetCheck += Time.deltaTime;
                if (timePassedSinceLastAttackTargetCheck >= checkForAttackTargetRate && !mandatoryFirstAttack)
                {
                    timePassedSinceLastAttackTargetCheck %= checkForAttackTargetRate;
                    CheckIfAttackTargetReachable();
                }
                if (attackAllowed)
                {
                    timePassedSinceLastAttack += Time.deltaTime;
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

        protected virtual void StartAttacking()
        {
            if (attackAllowed)
                return;
            attackAllowed = true;
            timePassedSinceLastAttack += UnityEngine.Random.Range(0f, 0.1f * data.AttackRate);
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

        private void DoActionForAllMaterials(Action<Material> Action)
        {
            foreach (Renderer ren in renderers)
            {
                foreach (Material mat in ren.materials)
                {
                    Action(mat);
                }
            }
        }
    }
}