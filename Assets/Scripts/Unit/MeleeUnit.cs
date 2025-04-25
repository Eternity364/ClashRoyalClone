
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

        protected Sequence sheathAnimation;

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
            swordInHand.SetActive(true);
            swordInTheBack.SetActive(false);
            SheathWeapon(false);
        }

        protected override void ClearAttackTarget(Unit unit) {
            base.ClearAttackTarget(unit);
            swordInHand.SetActive(false);
            swordInTheBack.SetActive(true);
            SheathWeapon(true);
        }

        private void SheathWeapon(bool active) {
            SwitchWeaponContext context = new SwitchWeaponContext();
            float callbackDelay = active ? 0.6f : 0.1f;
            context.type = active ? "Sheath" : "Unsheath";
            context.side = "Right";
            context.sheathLocation = "Back";
            context.leftWeapon = -1;
            context.rightWeapon = (int)Weapon.RightSword;
            rPGCharacterController.StartAction("SwitchWeapon", context);

            if (sheathAnimation != null) {
                sheathAnimation.Kill();
            }
            sheathAnimation = DOTween.Sequence();
            sheathAnimation.InsertCallback(callbackDelay, () => {
                swordInHand.SetActive(false);
                swordInTheBack.SetActive(true);
            });
        } 

        protected override void PerformAttack() {
            if (attackTarget != null) {
                // animator.SetInteger("TriggerNumber", 6);
                // animator.SetBool("Trigger", true);
                attackTarget.ReceiveAttack(attack);
            }
        }
    }
}
