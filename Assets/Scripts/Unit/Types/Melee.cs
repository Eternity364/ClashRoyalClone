
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using System.Collections;
using UnityEngine.Events;

namespace Units{
    public class Melee : Unit
    {    
        [SerializeField]
        private GameObject swordInHand;
        [SerializeField]
        private GameObject swordInTheBack;

        protected Sequence seq;

        public override void SetAttackTarget(Unit target) {
            bool hadTargetBefore = HasTarget;
            base.SetAttackTarget(target);
            SheathWeapon(false, hadTargetBefore);
        }
        
        protected override void ClearAttackTarget(Unit unit) {
            SheathWeapon(true);
            base.ClearAttackTarget(unit);
        }
        
        private void SheathWeapon(bool active, bool forceStartAttacking = false)
        {
            if (isDead)
                return;
            if (HasTarget == active)
            {
                if (forceStartAttacking)
                {
                    StartAttacking();
                }
                return;
            }
            SwitchWeaponContext context = new SwitchWeaponContext();
            float callbackDelay = active ? 0.6f : 0.1f;
            context.type = active ? "Sheath" : "Unsheath";
            context.side = active ? "None" : "Right";
            context.sheathLocation = "Back";
            context.leftWeapon = (int)Weapon.Relax;
            context.rightWeapon = active ? (int)Weapon.Relax : (int)Weapon.RightSword;
            rPGCharacterController.StartAction("SwitchWeapon", context);

            seq?.Kill();
            seq = DOTween.Sequence();
            seq.InsertCallback(callbackDelay, () =>
            {
                swordInHand.SetActive(!active);
                swordInTheBack.SetActive(active);
                if (active)
                    animator.SetBool("Trigger", true);
            });
            seq.InsertCallback(1, () =>
            {
                if (!active)
                    StartAttacking();
            });
        } 

        protected override void OnAttackRotationComplete() {}

        protected override void PerformAttack() {
            base.PerformAttack();

            if (attackTarget != null) {
                animator.SetInteger("TriggerNumber", 6);
                animator.SetBool("Trigger", true);

                if (seq != null) {
                    seq.Kill();
                }
                seq = DOTween.Sequence();
                seq.InsertCallback(0.5f, () => {
                    attackTarget.ReceiveAttack(data.Attack);
                });
            }
        }
    }
}
