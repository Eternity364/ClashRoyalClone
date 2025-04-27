
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using System.Collections;

namespace Assets.Scripts.Unit {
    public class MeleeUnit : Unit
    {    
        [SerializeField]
        private GameObject swordInHand;
        [SerializeField]
        private GameObject swordInTheBack;

        protected Sequence seq;

        void Start()
        {
            //StartCoroutine(Test());
        }

        IEnumerator Test() {
            
            yield return new WaitForSeconds(1f);
            SheathWeapon(false);
            yield return new WaitForSeconds(2f);
            SheathWeapon(true);
        }

        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) {
            base.Init(destination, bulletFactory, team, teamColor);
        }

        public override void SetAttackTarget(Unit target) {
            base.SetAttackTarget(target);
            SheathWeapon(false);
        }

        protected override void ClearAttackTarget(Unit unit) {
            base.ClearAttackTarget(unit);
            SheathWeapon(true);
        }

        private void SheathWeapon(bool active) {
            SwitchWeaponContext context = new SwitchWeaponContext();
            float callbackDelay = active ? 0.6f : 0.1f;
            context.type = active ? "Sheath" : "Unsheath";
            context.side = active ? "Dual": "Right";
            context.sheathLocation = "Back";
            context.leftWeapon = (int)Weapon.Unarmed;
            context.rightWeapon = active ? (int)Weapon.Unarmed : (int)Weapon.RightSword;
            rPGCharacterController.StartAction("SwitchWeapon", context);

            if (seq != null) {
                seq.Kill();
            }
            seq = DOTween.Sequence();
            seq.InsertCallback(callbackDelay, () => {
                swordInHand.SetActive(!active);
                swordInTheBack.SetActive(active);
            });            
            seq.InsertCallback(1, () => {
                if (!active)
                    StartAttacking();
            });
        } 

        protected override void OnAttackRotationComplete() {}

        protected override void PerformAttack() {
            if (attackTarget != null) {
                animator.SetInteger("TriggerNumber", 6);
                animator.SetBool("Trigger", true);

                if (seq != null) {
                    seq.Kill();
                }
                seq = DOTween.Sequence();
                seq.InsertCallback(0.5f, () => {
                    attackTarget.ReceiveAttack(attack);
                });
            }
        }
    }
}
