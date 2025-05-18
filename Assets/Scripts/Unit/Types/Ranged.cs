
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using RPGCharacterAnims;
using System;
using RPGCharacterAnims.Actions;

namespace Units{
    public class Ranged : Unit
    {    
        [SerializeField]    
        private GameObject arrow;

        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor)
        {
            base.Init(destination, bulletFactory, team, teamColor);

            SwitchWeaponContext context = new SwitchWeaponContext();
            context.type = "Instant";
            context.side = "None";
            context.sheathLocation = "Back";
            context.leftWeapon = -1;
            context.rightWeapon = (int)Weapon.TwoHandBow;
            rPGCharacterController.StartAction("SwitchWeapon", context);
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

        protected override void PerformAttack()
        {
            base.PerformAttack();

            if (attackTarget != null)
            {
                GameObject bullet = bulletFactory.Get();
                bullet.transform.localScale = arrow.transform.lossyScale;
                ArrowFlight arrowFlight = bullet.GetComponent<ArrowFlight>();
                NavMeshAgent attackTargetNavMesh = attackTarget.GetComponent<NavMeshAgent>();
                float attackTargetSize = attackTarget.Radius;
                Vector3 targetPosition = attackTarget.transform.position;

                TriggerBowAnimation(() =>
                {
                    bullet.SetActive(true);
                    arrowFlight.FlyArrow(arrow.transform.position, targetPosition, attackTargetSize, OnBulletFlyComplete);
                });

                void OnBulletFlyComplete()
                {
                    bullet.SetActive(false);
                    if (attackTarget != null)
                        attackTarget.ReceiveAttack(attack);
                }
            }
        }
    }
}
